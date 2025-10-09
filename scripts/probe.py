import requests
from bs4 import BeautifulSoup
from urllib.parse import urlparse

url = "https://liftmanual.com/muscle/abs/"
resp = requests.get(url, timeout=30)
resp.raise_for_status()
soup = BeautifulSoup(resp.text, "html.parser")
anchors = soup.select("div.index-block a[href^='https://liftmanual.com/']")
print('count', len(anchors))
for a in anchors[:10]:
    parsed = urlparse(a['href'])
    print(a.get_text(strip=True), parsed.path, parsed.path.count('/'))
