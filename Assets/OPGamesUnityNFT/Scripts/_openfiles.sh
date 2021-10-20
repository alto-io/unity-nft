#!/bin/sh

ctags -R .
vim `find . -name "*.cs"`
