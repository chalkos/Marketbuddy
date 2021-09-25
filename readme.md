# Marketbuddy

Plugin for XivLauncher/Dalamud to help with your day-to-day market operations.

## How to get

1. Dalamud settings -> Experimental
2. Add `https://raw.githubusercontent.com/Chalkos/Marketbuddy/api4/repo.json` and enable it
3. Install from the plugin list

## Commands

* `/mbuddy` plugin config

## Features

Simplifies updating one price:
* Click adjust price as per usual
  * The window to change price, the list of current items on the market and history of sales for that item open at once
  * Click one of the current items on the market
  * You price will be set 1 gil below and you'll be back at your items list
  * (the set price will be copied to clipboard)

Simplifies updating more items with the same price:
* Click adjust price, while holding CTRL
  * The value from the clipboard will be used for that item

Each feature can be configured in the plugin config.

Holding SHIFT prevents some automation, but you're better off just disabling whatever you don't want in the config.

## Changelog

* 0.2.1.0
  * `/mbuddy` shows/hides the config window
  * `/mbuddyconf` removed
  * New option to adjust stack size on the retainer sell list addon (because opening the config everytime is annoying)
* 0.2.0.0
  * API4 support
  * Add option to limit the stack size when posting new items