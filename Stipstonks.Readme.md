# Stipstonks
Stipstonks is a basic implementation of the `beursfuif` game. It has no advanced features like multiplayer between bars or integration with payment cards, but at least it looks good and the setup is relatively easy.

## General behavior
The current prices of all products are periodically updated based on the number of items sold since the last crash. On the chart window there is a progress bar that counts down to when the update happens.  
A second progress bar counts down to a crash. When a crash happens all prices are temporarily set to their minimum possible values. When the crash ends all prices are reset to their initial values and are updated periodically again like in the beginning. This cycle repeats until stopped manually.

## Usage
All you need to run `Stipstonks` is the executable itself and a file named `Data.json` that is placed right next to it. This second file contains the list of products/categories you are going to sell and some other configuration. Check the section below for a thorough explanation off all configuration options. An example configuration file is included when you download the app.

To start using the app you need to:
* Prepare the configuration file.
* Launch the app.
* Drag the chart window to the big screen.
* Click the `(De)maximize chart window` button on the input window to make the chart window full screen.
* Click the `Reset` button on the input window to ensure all values are properly set unless you want to continue from where you last left off.
* Click the `Start` button on the input window.
* For every order, enter the amounts of the products ordered. You will see the total price for convenience. To confirm the order, click the `Commit` button.
* At the end of the event click `Stop` or close the app.

You can at all times reset everything to the initial state by clicking the `Reset` button.
If you restart the app without resetting it will continue where it left off but the price update and crash countdown timers are reset.

## Configurations
The values described below are used to control the behavior of the app. Any values that are not mentioned here should never be changed by hand.

* `priceUpdateIntervalInSeconds`  
Type: integer  
The number of seconds between 2 price updates.

* `crashIntervalInSeconds`  
Type: integer  
The number of seconds between 2 stonk market crashes. Ideally it is a multiple of `priceUpdateIntervalInSeconds`.

* `crashDurationInSeconds`  
Type: integer  
When a crash happens, this is the amount of seconds that the stonk market remains in the crashed state before it is reset and continues as normal.

* `maxPriceDeviationFactor`  
Type: floating point  
Every product has a base price which is its initial price. This factor defines how much the calculated prices can deviate from the base price. E.g. when `maxPriceDeviationFactor` is 0.3 it means the calculated price of a product can be at most 30% lower or higher than the base price. For a product with a base price of €2.00 this means the price will always be in the €1.40 to €2.60 range.

* `priceResolutionInCents`  
Type: integer  
All prices are rounded to this resolution. E.g. when `priceResolutionInCents` is set to 5 all calculated prices are rounded to the nearest multiple of €0.05. Natural rounding is used.

* `allowPriceUpdatesDuringOrder`  
Type: boolean  
This setting controls the behavior of the input window when the prices are recalculated (including when caused by a crash) and an order is ongoing at the same time. An order is considered ongoing when any product has an amount greater than 0 entered on the input window.  
When this value is set to `false` the prices used to calculate the total price remain `frozen` to what they were when the first product was entered. It will keep using these prices until the order is committed.  
When this value is set to `true` then the total price will update in real time based on the periodic price updates.

* `windowBackgroundColor`  
Type: string in hexadecimal RGB color format, e.g. "#FF0000" for red  
The normal background color of the windows.

* `crashChartWindowBackgroundColor`  
Type: string in hexadecimal RGB color format, e.g. "#00FF00" for green  
The background color that the chart window has when the stonk market is in the crashed state. When the crash ends the chart window's background color is reset to `windowBackgroundColor`.

* `priceUpdateProgressBarColor`  
Type: string in hexadecimal RGB color format, e.g. "#0000FF" for blue  
The color of the progress bar that counts down to the next price update.

* `crashProgressBarColor`  
Type: string in hexadecimal RGB color format, e.g. "#0F0F0F" for grey  
The color of the progress bar that counts down to the next stonk market crash.

* `showCrashProgressBar`  
Type: boolean  
When set to `true`, the crash progress bar on the chart window will be visible.  
When set to `false`, the crash progress bar on the chart window will not be visible.

* `products`  
A list of products/categories. Every product has the following configuration options:
	* `name`  
    Type: string  
	The name of the product as shown on all windows.

	* `color`  
	Type: string in hexadecimal RGB color format, e.g. "#FFFFFF" for white  
	The color of the product's chart and input card.

	* `basePriceInCents`  
	Type: integer  
	The initial price of the product. Also the price that the product will reset to after a stonk market crash.