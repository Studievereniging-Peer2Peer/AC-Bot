# AC-Bot
De discord bot van de activiteitencommissie van Studievereniging Peer2Peer

### Belangrijke informatie voor deployment

1. Deze bot maakt gebruik van de lokale tijdzone van het systeem voor de reminders. Deze moet bij een deployment dus Amsterdam/GMT+1 zijn.
2. Deze bot maakt gebruik van de environment variabel 'DISCORD_API_KEY_AC_BOT' om te communiceren met de Discord gateway.

De bot kan hierna aangezet worden als volgt:
 1. (in de folder met de Dockerfile) build de container: docker build -t acbot .
 2. run de docker container: docker run -e DISCORD_API_KEY_AC_BOT=<BOT_SLEUTEL_HIER> --name <naam container> acbot

de '-e' tag zet de environment variabel, in dit geval de sleutel om met de Discord gateway te communiceren via de bot.
 