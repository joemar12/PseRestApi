# PseRestApi

A simple REST API to query stock prices in PSE

This is using internal API calls being fired by PSE's public website, wrapped in a .Net REST API

Archiving feature is now live.  
Oldest trade date is 04/04/2023

check it out here: https://psestockprice.azurewebsites.net

sample request for latest price: 
`https://psestockprice.azurewebsites.net/stockprice/FMETF`  
sample request for latest price as of a given date: 
`https://psestockprice.azurewebsites.net/stockprice/FMETF/2023-04-05`

sample response:
```
{
    "securityName": "FIRST METRO PHILIPPINE EQUITY EXCHANGE TRADED FUND, INC.",
    "symbol": "FMETF",
    "price": [
        {
            "currency": "PHP",
            "price": 101.8
        }
    ],
    "lastTradeDate": "2023-04-04T00:00:00",
    "percentChange": 0.39448,
    "volume": 5260
}
```
