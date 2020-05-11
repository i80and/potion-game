GDSCRIPT_FILES=$(wildcard scripts/*.gd)

.PHONY: lint format

lint:
	gdlint $(GDSCRIPT_FILES)

format:
	gdformat $(GDSCRIPT_FILES)