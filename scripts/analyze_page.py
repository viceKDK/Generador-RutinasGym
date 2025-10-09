import requests
from bs4 import BeautifulSoup
url = "https://liftmanual.com/barbell-rollout/"
resp = requests.get(url, timeout=30)
resp.raise_for_status()
soup = BeautifulSoup(resp.text, "html.parser")
content = soup.select_one('article')
for p in content.select('p'):
    text = p.get_text(strip=True)
    if text:
        print(text)
