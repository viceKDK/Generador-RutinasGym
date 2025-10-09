import csv
import json
import re
import sys
import time
import unicodedata
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Tuple
from urllib.parse import urljoin, urlparse

import requests
from bs4 import BeautifulSoup
from googletrans import Translator
from requests.adapters import HTTPAdapter, Retry

BASE_URL = "https://liftmanual.com"
MUSCLE_INDEX_URL = f"{BASE_URL}/muscle/"
PROJECT_ROOT = Path(__file__).resolve().parents[1]
OUTPUT_ROOT = PROJECT_ROOT / "docs" / "ejercicios"
METADATA_FILE = OUTPUT_ROOT / "metadata.csv"
TRANSLATION_CACHE_FILE = OUTPUT_ROOT / "translations_cache.json"

USER_AGENT = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) "
    "Chrome/128.0.0.0 Safari/537.36"
)

session = requests.Session()
retry_strategy = Retry(
    total=5,
    backoff_factor=1.0,
    status_forcelist=[429, 500, 502, 503, 504],
    allowed_methods=["GET"],
)
session.mount("https://", HTTPAdapter(max_retries=retry_strategy))
session.headers.update({"User-Agent": USER_AGENT})

translator = Translator()
translation_cache: Dict[str, str] = {}


def load_translation_cache() -> None:
    if TRANSLATION_CACHE_FILE.exists():
        try:
            translation_cache.update(json.loads(TRANSLATION_CACHE_FILE.read_text(encoding="utf-8")))
        except json.JSONDecodeError:
            print("Warning: translation cache file is corrupted, ignoring.", file=sys.stderr)


def save_translation_cache() -> None:
    TRANSLATION_CACHE_FILE.parent.mkdir(parents=True, exist_ok=True)
    TRANSLATION_CACHE_FILE.write_text(json.dumps(translation_cache, ensure_ascii=False, indent=2), encoding="utf-8")


INVALID_FS_CHARS = re.compile(r"[<>:\"/\\|?*]")
MULTIPLE_SPACES = re.compile(r"\s+")


def sanitize_for_fs(text: str, *, allow_spaces: bool = True, max_len: int = 120) -> str:
    if not text:
        return "sin_nombre"
    normalized = unicodedata.normalize("NFKC", text)
    normalized = normalized.replace("\n", " ").replace("\r", " ")
    normalized = MULTIPLE_SPACES.sub(" ", normalized).strip()
    normalized = INVALID_FS_CHARS.sub("", normalized)
    normalized = normalized.strip().strip('.')
    if not allow_spaces:
        normalized = normalized.replace(" ", "_")
    if len(normalized) > max_len:
        normalized = normalized[:max_len].rstrip()
    return normalized or "sin_nombre"


def translate_text(text: str) -> str:
    key = text.strip()
    if not key:
        return text
    cached = translation_cache.get(key)
    if cached:
        return cached
    for attempt in range(3):
        try:
            result = translator.translate(key, src="en", dest="es")
            translated = result.text.strip()
            if translated:
                translation_cache[key] = translated
                time.sleep(0.35)
                return translated
        except Exception as exc:
            print(f"Translation error for '{key}' (attempt {attempt + 1}): {exc}", file=sys.stderr)
            time.sleep(2.0 * (attempt + 1))
    translation_cache[key] = key
    return key


def get_soup(url: str) -> BeautifulSoup:
    response = session.get(url, timeout=30)
    response.raise_for_status()
    return BeautifulSoup(response.text, "html.parser")


def collect_muscle_links() -> List[Tuple[str, str]]:
    soup = get_soup(MUSCLE_INDEX_URL)
    links: Dict[str, str] = {}
    for anchor in soup.select("a[href^='https://liftmanual.com/muscle/']"):
        href = anchor.get("href")
        if not href:
            continue
        parsed = urlparse(href)
        if parsed.path.count('/') != 3:
            continue
        name = anchor.get_text(strip=True)
        if not name:
            continue
        links[href] = name
    return sorted(((name, href) for href, name in links.items()), key=lambda pair: pair[0])


EXCLUDED_PREFIXES = (
    "/muscle/",
    "/equipment/",
    "/strength/",
    "/stretching/",
    "/cardio/",
    "/routines/",
    "/guides/",
    "/category/",
    "/tag/",
    "/author/",
    "/blog/",
)


def collect_exercise_links(muscle_url: str) -> List[Tuple[str, str]]:
    soup = get_soup(muscle_url)
    exercises: Dict[str, str] = {}
    for anchor in soup.select("div.index-block a[href^='https://liftmanual.com/']"):
        href = anchor.get("href")
        if not href:
            continue
        parsed = urlparse(href)
        path = parsed.path
        if not path or not path.endswith('/'):
            continue
        if any(path.startswith(prefix) for prefix in EXCLUDED_PREFIXES):
            continue
        if path.count('/') != 2:
            continue
        name = anchor.get_text(strip=True)
        if not name:
            continue
        exercises[href] = name
    return sorted(((name, href) for href, name in exercises.items()), key=lambda pair: pair[0])


def pick_description(soup: BeautifulSoup) -> Optional[str]:
    for heading in soup.find_all(["h2", "h3"]):
        if "description" in heading.get_text(strip=True).lower():
            para = heading.find_next("p")
            if para:
                text = para.get_text(strip=True)
                if text:
                    return text
    meta = soup.find("meta", attrs={"name": "description"})
    if meta and meta.get("content"):
        return meta["content"].strip()
    meta = soup.find("meta", attrs={"property": "og:description"})
    if meta and meta.get("content"):
        return meta["content"].strip()
    for para in soup.select("p"):
        text = para.get_text(strip=True)
        if text and "home" not in text.lower():
            return text
    return None


IMG_EXT_PREFERENCE = [".webp", ".jpg", ".jpeg", ".png", ".gif"]


def pick_image_url(soup: BeautifulSoup) -> Optional[str]:
    candidates: List[Tuple[int, str]] = []
    for img in soup.select("img"):
        src = (
            img.get("data-src")
            or img.get("data-lazy-src")
            or img.get("data-large_image")
            or img.get("src")
        )
        if not src:
            continue
        src = src.strip()
        if src.startswith("data:"):
            continue
        if "Lift-Manual" in src:
            continue
        parsed = urlparse(src)
        ext = Path(parsed.path).suffix.lower()
        if not ext or ext not in IMG_EXT_PREFERENCE:
            continue
        if not src.startswith("http"):
            src = urljoin(BASE_URL, src)
        preference_index = IMG_EXT_PREFERENCE.index(ext)
        candidates.append((preference_index, src))
    if not candidates:
        return None
    candidates.sort(key=lambda item: item[0])
    return candidates[0][1]


def download_file(url: str, dest: Path) -> None:
    dest.parent.mkdir(parents=True, exist_ok=True)
    response = session.get(url, timeout=60)
    response.raise_for_status()
    dest.write_bytes(response.content)


def ensure_unique_path(base_path: Path) -> Path:
    if not base_path.exists():
        return base_path
    counter = 1
    while True:
        candidate = base_path.parent / f"{base_path.name}-{counter}"
        if not candidate.exists():
            return candidate
        counter += 1


def ensure_unique_filename(directory: Path, filename: str, extension: str) -> Path:
    candidate = directory / f"{filename}{extension}"
    if not candidate.exists():
        return candidate
    counter = 1
    while True:
        candidate = directory / f"{filename}-{counter}{extension}"
        if not candidate.exists():
            return candidate
        counter += 1


def main() -> None:
    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)
    load_translation_cache()

    muscle_links = collect_muscle_links()
    print(f"Found {len(muscle_links)} muscle groups")

    processed_slugs: set[str] = set()
    records: List[Tuple[str, str, str, str, str]] = []

    for muscle_name_en, muscle_url in muscle_links:
        muscle_name_es = translate_text(muscle_name_en)
        muscle_folder_name = sanitize_for_fs(muscle_name_es, allow_spaces=True)
        muscle_folder = OUTPUT_ROOT / muscle_folder_name
        muscle_folder.mkdir(parents=True, exist_ok=True)

        exercises = collect_exercise_links(muscle_url)
        print(f"Processing {len(exercises)} exercises for {muscle_name_en} -> {muscle_name_es}")

        for exercise_name_en, exercise_url in exercises:
            slug = urlparse(exercise_url).path.rstrip('/').split('/')[-1]
            if slug in processed_slugs:
                print(f"  Skipping already processed exercise {exercise_name_en} ({slug})")
                continue
            processed_slugs.add(slug)

            try:
                soup = get_soup(exercise_url)
            except Exception as exc:
                print(f"  Failed to fetch {exercise_url}: {exc}", file=sys.stderr)
                continue

            description = pick_description(soup) or "Sin descripción disponible"
            image_url = pick_image_url(soup)
            if not image_url:
                print(f"  No image found for {exercise_name_en}", file=sys.stderr)
                continue

            exercise_name_es = translate_text(exercise_name_en)
            exercise_folder_name = sanitize_for_fs(exercise_name_es, allow_spaces=True)
            exercise_folder = ensure_unique_path(muscle_folder / exercise_folder_name)
            exercise_folder.mkdir(parents=True, exist_ok=True)

            desc_filename = sanitize_for_fs(description, allow_spaces=True, max_len=180)
            file_extension = Path(urlparse(image_url).path).suffix or ".jpg"
            image_path = ensure_unique_filename(exercise_folder, desc_filename, file_extension)

            try:
                download_file(image_url, image_path)
                print(f"  Saved {exercise_name_es} -> {image_path.relative_to(PROJECT_ROOT)}")
            except Exception as exc:
                print(f"  Failed to download image {image_url}: {exc}", file=sys.stderr)
                continue

            records.append((
                muscle_name_es,
                exercise_name_es,
                exercise_name_en,
                description,
                str(image_path.relative_to(PROJECT_ROOT))
            ))
            time.sleep(0.6)

    if records:
        with METADATA_FILE.open("w", encoding="utf-8", newline="") as fh:
            writer = csv.writer(fh)
            writer.writerow(["Grupo muscular (es)", "Ejercicio (es)", "Ejercicio (en)", "Descripción", "Ruta imagen"])
            writer.writerows(records)
        print(f"Metadata saved to {METADATA_FILE.relative_to(PROJECT_ROOT)}")

    save_translation_cache()
    print("Done")


if __name__ == "__main__":
    try:
        main()
    finally:
        save_translation_cache()
