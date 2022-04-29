# Marketbuddy

Plugin for XivLauncher/Dalamud to help with your day-to-day market operations.

## How to get

1. Dalamud settings -> Experimental
2. Add `https://raw.githubusercontent.com/Chalkos/Marketbuddy/main/repo.json` and enable it
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

* 0.2.2.10
  * fix: limit max stack size to 9999 (up from 999)
* 0.2.2.9
  * fix: buying items will no longer show an error message
* 0.2.2.8
  * fix: also allow undercut to be 0
* 0.2.2.7
  * fix: prevent undercut from being set to less than 1
  * new: adds undercut to the overlay
  * fix: overlay no longer gets disabled when not limiting stack size
  * fix: price set will always be between 1gil and 999999999gil
* 0.2.2.6
  * fixes a crash when quickposting items using CTRL
* 0.2.2.5
  * option: use SHIFT to open/not open the price history (previously only used to skip opening)
* 0.2.2.4
  * allow setting a custom undercut amount (thanks xPumaa)
  * option: use ALT to open/not open the price history
  * some other UI changes
  * fix a bug that (with a certain configuration setup) would close the item window and leave the price list open
* 0.2.2.3
  * set api version 6 (no code changes)
* 0.2.2.2
  * set api version 5 (no code changes)
* 0.2.2.1
  * api4 branch removed
    * use the json from the main branch (it's above) to get updates
* 0.2.1.0
  * `/mbuddy` shows/hides the config window
  * `/mbuddyconf` removed
  * New option to adjust stack size on the retainer sell list addon (because opening the config everytime is annoying)
* 0.2.0.0
  * API4 support
  * Add option to limit the stack size when posting new items