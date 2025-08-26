#!/usr/bin/env bash

source ./CONFIG.inc

clean() {
	rm $FILE
	if [ ! -d Archive ] ; then
		rm -f Archive
		mkdir Archive
	fi
}

pwd=$(pwd)
FILE=${pwd}/Archive/$PACKAGE-$VERSION${PROJECT_STATE}-NoParts.zip
echo $FILE
clean
zip -r $FILE ./GameData/* -x ".*"
zip $FILE INSTALL.md
zip -d $FILE __MACOSX "**/.DS_Store" "GameData/HLAirshipsCore/Parts/*"
cd $pwd
