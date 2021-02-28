__Introduction

Burl est un logiciel qui permet de tester l'existence de fichiers à partir d'une URL.
Le but est de lister rapidement des dossiers ou fichiers susceptibles d'être intéressants qui n'auraient pas été trouvés par un web crawler classique.

Burl teste toutes les URLs possibles en combinant les listes des URLs (urls.txt), de noms de fichiers/dossiers (files.txt), et d'extensions (dans Burl2.exe.config)

Burl affiche et écrit les résultats (selon le niveau de verbosité) dans un fichier results.txt


__Fichiers nécessaires

Burl.exe.config : Fichier de configuration (voir la partie Options paramètrables)
urls.txt : Liste d'URL à scanner, une URL par ligne
files.txt : noms des fichiers et dossiers à vérifier, un nom par ligne

index|4 : indique un niveau de recherche à appliquer pour cet élément
%config.inc : élément testé tel quel et sans extension (ici "config.inc")
#commentaire : ligne ignorée


__Options paramètrables

proxy : Indiquer si Burl doit passer par un proxy
proxyUser : Utilisateur pour le proxy (par défaut, les paramètres système)
proxyPassword : Mot de passe pour le proxy (par défaut, les paramètres système)
proxyHost : ip/hostname du proxy (par défaut, les paramètres système)

waitDelay : Durée en millisecondes d'attente entre deux requêtes. Par défaut 50.
timeout : Durée en millisecondes avant abandon de la requête. Par défaut 1000.
method : Méthode de la requête HTTP. Par défaut HEAD.
userAgent : User Agent envoyé par Burl. Par défaut un user agent de Firefox.

searchIntensity : Indiquer le degré d'intensité de recherche, voir le paramètre extensions.
outputVerbose : Indiquer la verbosité de Burl :
1 : Seules les réponses OK (code HTTP 200) sont loggées.
2 : Toutes les réponses, sauf 404 (page introuvable) sont loggées.
3 : Toutes les réponses sont loggées.

extensions : Les extensions à tester. Les extensions sont séparées par une virgule, et les différents degrés d'intensité par une nouvelle ligne.
