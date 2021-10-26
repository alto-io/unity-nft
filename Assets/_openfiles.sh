#!/bin/sh

cd OPGamesUnityNFT/Scripts/
ctags -R .
vim `find . -name "*.cs"`
