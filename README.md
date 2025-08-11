# wootowoo


You will need to add an appsettings.json file like this:

{
  "Destination": {
    "Uri": "https://destination.installation.com",
    "Key": "destination woocommerce api key",
    "Secret": "destination woocommerce api secret",
    "WordPressUser": {
      "ApplicationPasswordName": "admin",
      "ApplicationPassword": "destination WordPress api key"
    }
  },
  "Source": {
    "Uri": "https://source.installation.com",
    "Key": "source woocommerce api key",
    "Secret": "not required",
    "WordPressUser": "not required",
  }
}