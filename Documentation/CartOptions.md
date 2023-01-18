# Cart Product Options

Cart product options are extra user data that can be added to each product item in the cart.  Each control must have an xpath of "genxml/cartoptions/\<name\>"  

If the code below is added to the "CartListBody.cshtml" it allows the user to add text data onto the product entry in the cart.  
*Any control must be added to the product item in the list.  The same place as the QTY field in the default cart templates.*  


```
<div>
    @TextBox(new SimplisityInfo(cItem.Record),"genxml/cartoptions/testtext1"," class='w3-input w3-border'","",false,lp1,"","text")
    @TextBox(new SimplisityInfo(cItem.Record), "genxml/cartoptions/testtext2", " class='w3-input w3-border'","",false,lp1,"","text")
    @TextBox(new SimplisityInfo(cItem.Record), "genxml/cartoptions/testtext3", " class='w3-input w3-border'","",false,lp1,"","text")
</div>
```

The above TextBox controls will create a cart XML data structure of:
```
<genxml>
  <products list="true">
    <genxml>
      <key>6*d_qWAJD0RP4*</key>
      <qty datatype="double">1</qty>
      <productid>6</productid>
      <modelid>d_qWAJD0RP4</modelid>
      <index>0</index>
      <cartoptions>
        <testtext1>cartoption1</testtext1>
        <testtext2>cartoption2</testtext2>
        <testtext3>cartoption3</testtext3>
      </cartoptions>
    </genxml>
  </products>
  <textbox>
    <firstname />
    <lastname />
    <email />
    <phone />
    <company />
  </textbox>
  <hidden>
    <subtotal>1200</subtotal>
    <shippingtotal>0</shippingtotal>
    <total>1200</total>
  </hidden>
</genxml>
```