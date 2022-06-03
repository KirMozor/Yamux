#!/bin/bash

arch=`uname -m`

if [ "$EUID" -ne 0 ]
  then echo "Please run as root"
  exit
else
	if [[ -f "/usr/bin/unzip" ]]
	then
		if [[ -f "/usr/bin/curl" ]]
		then
			mkdir libbass
			case "$arch" in
			i686)
				echo "i686"
				curl -o "libbass_i686.zip" http://www.un4seen.com/files/bass24-linux.zip
				unzip libbass_i686.zip -d libbass
				cp libbass/libbass.so /usr/lib/libbass.so
			;;
			x86_64)
				echo "x86_64"
				curl -o "libbass_x86_64.zip" http://www.un4seen.com/files/bass24-linux.zip
				unzip libbass_x86_64.zip -d libbass
				cp libbass/x64/libbass.so /usr/lib/libbass.so
			;;
			armv6h|armv7h)
				echo "armv6h|armv7h"
				curl -o "libbass_armv6h_armv7h.zip" http://www.un4seen.com/files/bass24-linux-arm.zip
				unzip libbass_armv6h_armv7h.zip -d libbass
				cp libbass/hardfp/libbass.so /usr/lib/libbass.so
			;;
			aarch64)
				echo "aarch64"
				curl -o "libbass_aarch64.zip" http://www.un4seen.com/files/bass24-linux-arm.zip
				unzip libbass_aarch64.zip -d libbass
				cp libbass/aarch64/libbass.so /usr/lib/libbass.so
			;;
			esac

			cp libbass/bass.h /usr/include/bass.h
			mkdir /usr/share/doc/libbass
			cp libbass/bass.chm /usr/share/doc/libbass/bass.chm
			rm -rf libbass
		else
			echo "Please install curl"
		fi
	else
		echo "Please install unzip"
	fi
fi
