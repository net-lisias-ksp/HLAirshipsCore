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
FILE=${pwd}/Archive/$PACKAGE-$VERSION${PROJECT_STATE}.zip
echo $FILE
clean
zip -r $FILE ./GameData/* -x ".*"
zip -r $FILE ./Ships/* -x ".*"
zip $FILE INSTALL.md
zip -d $FILE __MACOSX "**/.DS_Store"
cd $pwd
