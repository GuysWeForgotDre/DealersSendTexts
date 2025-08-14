# Dealers Send Texts
![Dealers text you updates as they make deals, hit product / cash thresholds, and a daily summary.](https://github.com/GuysWeForgotDre/DealersSendTexts/blob/main/Images/Message%20Examples.png?raw=true)

Dealers text you updates as they make deals, hit product / cash thresholds, and a daily summary. Also monitors for incapacitated NPCs and dealer pathing issues. Supports `v0.4.0 Cartel beta` and `v0.3.6f6 stable`.

Options are configurable globally or on a per-dealer basis. Preferences can be adjusted using Mod Manager Phone App, or editing `/UserData/DealersSendText/Config.cfg`.

- **Requires:** [MelonLoader](https://melonwiki.xyz/#/)
- **Recommended:** [Mod Manager Phone App](https://www.nexusmods.com/schedule1/mods/397) | [Timestamp Text Messages](https://thunderstore.io/schedule-i/p/Dre/Timestamp_Texts)

---

## General Information
### Installation
- Drop the .dll into `%ScheduleOne Install%/Mods/`.
- Delete to uninstall.
### Features
- **`Detailed Dealer Tracking`**
  - Dealers log their customers, status updates, deal locations, time / day, and inventory / cash.
  - Realtime tracking of position and health status of dealers and customers.
- **`Highly Configurable`**
  - Send a variety of update, summary, and alert texts; see list below.
  - All options can be set globally, with per-dealer overrides available.
  - Automatically integrates with Phone Mod Manager if available, or simply edit `Config.cfg` file.
- **`Export Sales Data (json)`**
  - Automatically saves running log of all sales by dealer to export / analyze.
  - Includes customer, product, price, location, date, time, and status.
  - Includes cross reference of locations by deal, customer, and dealer.
  - Data saved to `%Save Directory%/Dealers Send Texts.json`
- **`Dynamic, Future-Proof Build`**
  - Dealers detected from ScheduleOne.Economy.Dealer class -- automatically compatible with newly added dealers.
  - Minimal patches into game code, mostly custom logic -- low chance to break on game update; easy to fix if it does.

---

## Send Text Options
*All configurable globally, with optional per-dealer override*

### Deal Update
- **`Deal Started`** Customer name, deal location and window, product amount and cost, and straight line distance from current location.
  - *Note:* Distance is only a reference; dealer may not head directly there next, and it doesn't account for pathing regardless.
- **`Deal Success`** Customer name, sale amount, time required, and remaining deals.
- **`Deal Failed`** Customer name, failure reason, lost potential, distance, and remaining deals.

<img src="https://github.com/GuysWeForgotDre/DealersSendTexts/blob/main/Images/Failed%20Deals.png?raw=true" width=400>

### Status Alert
- **`Customer Injury Alert`** Send text when customer is incapacitated.
  - Knocked out customers unavailable that day; dead customers unavailable for **3 days**.
- **`Dealer Injury Alert`** Send text when dealer is incapacitated.
  - Knocked out dealers unavailable that day; dead dealers unavailable for **2 days**.
- **`Dealer Stuck Alert`** Send text when dealer hasn't moved from one spot for a period of time.
  - Customize number of failed move checks and maximum radius to be considered immobile. 
- **`Navigation Alert`** Send text when a new destination is set. Includes cooldown to prevent spam.
### Inventory Alert
- **`Low Product Count`** Send text when total product below specified amount.
  - Includes cash, location they are leaving from, and straight line distance to player.
- **`High Cash Alert`** Send text when cash above specified amount.
  - Includes count, location they are leaving from, and straight line distance to player.
### Nightly Summary
- **`Deal Summary`** Count of successful and failed deals that day, total products sold and cash made.
- **`Customer Log`** List of last completed deal time by customer.
- **`Location Log`** Number of deals at each named location.
- **`Failure Log`** Customer and Location of incompleted deals.
### Color Coded Icons
<img src="https://github.com/GuysWeForgotDre/DealersSendTexts/blob/main/Images/Icon%20Types.png?raw=true" width=200>

*Icons show as a colored square in notifications if enabled.*

---

## Reference
### Update 2.0.0
- **`Bugs / Fixes`**
  - Better cleanup of data when game unloads, prevent potential issue when switching saves without exiting game.
- **`New Features`**
  - Support for Cartel update (gamme v0.4.0 beta) -- ensures methods only grab player dealers and not the cartel's.
  - Separate data per save file.
### Update 1.2.0
- **`Bugs / Fixes`**
  - Fixed jars and bricks counting as 1. Dealers now report same total count as shown in Dealer app.
  - Now accounts for dealer cut, and quick delivery bonus when applicable.
- **`New Features`**
  - Master Settings to configure all dealers at once. Optional per-deal override available.
  - Provide specific reason for deal failures, see list.
  - Option to alert if dealer or customer is injured or dies.
  - Option to alert if dealer appears stuck for prolonged time.
  - Option to notify on new navigation destination set.
  - Track each deal location by deals started / success / failure, customer usage, and dealer usage.
  - Seamlessly save internal data when game is saved, auto load at startup.
### Source Code
- This program is open source under the `MIT license`. I encourage you to learn from it or use it in your own creations.
- [Github Repository](https://github.com/GuysWeForgotDre/DealersSendTexts/)
### Mod Links
[Thunderstore](https://thunderstore.io/schedule-i/p/Dre/DealersSendTexts) | [Nexus Mods](https://www.nexusmods.com/schedule1/mods/1133)
### Contact
Discord: `OnlyMurdersSometimes` | Github: `GuysWeForgotDre`