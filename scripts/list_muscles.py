import requests
from bs4 import BeautifulSoup
url = "https://liftmanual.com/muscle/"
resp = requests.get(url, timeout=30)
resp.raise_for_status()
soup = BeautifulSoup(resp.text, "html.parser")
links = []
for a in soup.select('a[href^="https://liftmanual.com/muscle/"]'):
    href = a['href']
    if href.count('/') == 5:  # ends with slash
        links.append((a.get_text(strip=True), href))
unique = {}
for text, href in links:
    unique[href] = text
for href, text in sorted(unique.items(), key=lambda x: x[1]):
    print(text, href)
