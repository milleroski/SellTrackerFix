# A fix to the mod SellTracker
This is a fix which gets rid of the quota only updating for the host. The cause of this bug is that the itemsOnCounter variable only updates for the host, and not for the clients.
