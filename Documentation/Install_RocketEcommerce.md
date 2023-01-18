# Install RocketEcommerceAPI



1. Install clear DNN website.

2. Install Rocket API  https://github.com/SesameRocket/DNNrocket/releases

3. Install RocketEcommerceAPI https://github.com/RocketEcommerceAPI/RocketEcommerceAPI/releases

4. Add "RocketSystem" module onto the home page.

   

The Portal zero is kept as an administration portal.   Any new shop created will become a new portal and will have it's own "Engine" URL.

1. Enter "Rocket E-commerce" and Add a "Portal Shop".
2. Save Portal shop and create new engine URL.
3. Create URL in DNS or host.ini, pointing to the correct server IP.

#### Themes

Themes are NOT installed with the Rocket Ecommmerce.  Until this is fixed we need to install the Themes manually.

Take the "rocketecommerceapi" folder from GitHub https://github.com/SesameRocket/RocketThemes

Put this folder onto the server under.  \DesktopModules\RocketThemes\

Restart the App Pool and the Themes should be visible in the Shop Management Panel.  (See Below)



#### Remote DNN module

The DNN remote module requests the html from the rocket ecommerce engine, which has been setup with the above URL.

The DNN version of the module expects a code string to create all the setting.  Settings can be setup manually, but this method saves time.

In the Portal Shop click the "Admin" button.



![image-20210408155755929](C:\Nevoweb\Projects\DesktopModules\DNNrocketModules\RocketEcommerceAPI\Documentation\img\image-20210408155755929.png)



This will ask you to login, login with the host or manager login (Needs to be given authorization).

Save the Security Key and  Code, these will be entered into the remote module.  If any settings change, then this code will change and need to be entered into the remote module again.

![image-20210408160245332](C:\Nevoweb\Projects\DesktopModules\DNNrocketModules\RocketEcommerceAPI\Documentation\img\image-20210408160245332.png)



Add the "" module onto a DNN page.  Open the settings tab of the remote DNN module and add the secrey key and code.

![image-20210408160549342](C:\Nevoweb\Projects\DesktopModules\DNNrocketModules\RocketEcommerceAPI\Documentation\img\image-20210408160549342.png)

The module should them display the selected e-commerce theme html.



