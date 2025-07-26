# Dealers Send Texts
Dealers will send you updates as they make deals, hit product / cash thresholds, and a daily summary. Each option is configurable on a per-dealer basis. Preferences can be adjusted using Mod Manager Phone App, or editing /UserData/DealersSendText.cfg.

## Available Options
- Send text when deal started
  - Provides customer name, deal location and window, product amount and cost, and straight line distance from current location.
  - Note that distance is only a reference, as they may not head directly there next, and it doesn't account for pathing regardless.
- Send text when deal completed
  - Provides customer name, sale amount, time required, and remaining deals.
- Send text when deal expired
  - Provides customer name, lost potential, straight line distance from current location, and remaining deals.
- Optional notification for the above deal texts. If disabled, texts will silently show up at the top of your message list.
- Send alert text when total product count below threshold
  - Configurable amount per dealer. Also send current cash, location they are leaving from, and straight line distance to player.
  - Only counts total quantity of items, e.g. a brick and a baggie both add 1. May be improved in the future.
- Send alert text when cash on hand above threshold
  - Configurable amount per dealer. Also send product count, location they are leaving from, and straight line distance to player.
- Optional notification for the above alerts. If disabled, texts will silently show up at the top of your message list.

## Other Features
- Color coded icons for quick refence:
  - Blue Triangle: Deal started
  - Green Check: Deal completed
  - Red 'X': Deal failed (timed out)
  - Orange '?!': Product / cash alert
  - Purple List: Daily Summary
- The color from the icons shows in the text notifications if enabled.
