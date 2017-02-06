#!/bin/sh -e
for file in bin/*.symbols.nupkg
do
  mv "$file" "${file%.symbols.nupkg}.nupkg"
done