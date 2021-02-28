__Introduction

Burl est un logiciel qui permet de tester l'existence de fichiers � partir d'une URL.
Le but est de lister rapidement des dossiers ou fichiers susceptibles d'�tre int�ressants qui n'auraient pas �t� trouv�s par un web crawler classique.

Burl teste toutes les URLs possibles en combinant les listes des URLs (urls.txt), de noms de fichiers/dossiers (files.txt), et d'extensions (dans Burl2.exe.config)

Burl affiche et �crit les r�sultats (selon le niveau de verbosit�) dans un fichier results.txt


__Fichiers n�cessaires

Burl.exe.config : Fichier de configuration (voir la partie Options param�trables)
urls.txt : Liste d'URL � scanner, une URL par ligne
files.txt : noms des fichiers et dossiers � v�rifier, un nom par ligne

index|4 : indique un niveau de recherche � appliquer pour cet �l�ment
%config.inc : �l�ment test� tel quel et sans extension (ici "config.inc")
#commentaire : ligne ignor�e


__Options param�trables

proxy : Indiquer si Burl doit passer par un proxy
proxyUser : Utilisateur pour le proxy (par d�faut, les param�tres syst�me)
proxyPassword : Mot de passe pour le proxy (par d�faut, les param�tres syst�me)
proxyHost : ip/hostname du proxy (par d�faut, les param�tres syst�me)

waitDelay : Dur�e en millisecondes d'attente entre deux requ�tes. Par d�faut 50.
timeout : Dur�e en millisecondes avant abandon de la requ�te. Par d�faut 1000.
method : M�thode de la requ�te HTTP. Par d�faut HEAD.
userAgent : User Agent envoy� par Burl. Par d�faut un user agent de Firefox.

searchIntensity : Indiquer le degr� d'intensit� de recherche, voir le param�tre extensions.
outputVerbose : Indiquer la verbosit� de Burl :
1 : Seules les r�ponses OK (code HTTP 200) sont logg�es.
2 : Toutes les r�ponses, sauf 404 (page introuvable) sont logg�es.
3 : Toutes les r�ponses sont logg�es.

extensions : Les extensions � tester. Les extensions sont s�par�es par une virgule, et les diff�rents degr�s d'intensit� par une nouvelle ligne.
