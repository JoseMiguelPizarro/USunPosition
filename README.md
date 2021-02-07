[![openupm](https://img.shields.io/npm/v/com.rojo.usunposition?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.rojo.usunposition/) [![MIT Licence](https://badges.frapsoft.com/os/mit/mit.svg?v=103)](https://opensource.org/licenses/mit-license.php) [![Open Source Love](https://badges.frapsoft.com/os/v1/open-source.svg?v=103)](https://github.com/ellerbrock/open-source-badges/)


# USunPosition

This is an astronomically accurate Sun positioning system for Unity. It implements de PSA algorithm: https://www.sciencedirect.com/science/article/abs/pii/S0038092X00001560


## How To Install

Add the following dependency in your manifest.json
``` json
{
  "dependencies": {
      
    ...
    "com.rojo.usunposition": "https://github.com/JoseMiguelPizarro/usunposition.git"
    ...
  }
}
```

Or install it via the <b>OpenUPM</b> CLI https://openupm.com/packages/com.rojo.usunposition/

## How to Use

You can get the spherical coordinates for the sun specifying the date time and real world position from where you want to compute the Sun position.

``` cs
SphericalCoordinates sunCoordinates = GetSunCoordinates(year,month, day, hour, longitude, latitude);
```

Additionally you can alignt to the Sun's light direction directly by using <b>AlignToLightDirection</b> and passing the transform that you want to align

``` cs
SunPosition.AlignToLightDirection(directionalLight, year, month, day, hour, latitude, longitude);
```

![sunpositionGif](https://s2.gifyu.com/images/sunposition.gif)

## Licence

* MIT