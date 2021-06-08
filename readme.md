<img src="https://raw.githubusercontent.com/isketch-app/iSketch.app/master/wwwroot/static/images/isketch.svg">
<h1>iSketch.app</h1>
<p>
  iSketch was a browser-based drawing game similar to Pictionary.
  It was launched by Robert Wahlstedt on June 15, 1999, and is written in Adobe Shockwave.
  The original site is still available at iSketch.net
</p>
<p>
  This project is an open source HTML5 / WebSocket driven re-write of the original game! (Not affiliated with the original authors of iSketch.net)
</p>
<p>
  Play online at <a href="https://isketch.app">isketch.app</a>!
</p>
<h2>Hosting your own instance of iSketch.app:</h2>

```bash
mkdir iSketch.app
cd iSketch.app
wget -O docker-compose.yml https://raw.githubusercontent.com/isketch-app/iSketch.app/master/docker-compose.yml
docker compose up -d
```

Browse to http://docker-host:8080
