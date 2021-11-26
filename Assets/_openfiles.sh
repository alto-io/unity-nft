#!/bin/sh

ctags -R .
vim `find ./OPGamesUnityNFT/Scripts/ -name "*.cs"`
