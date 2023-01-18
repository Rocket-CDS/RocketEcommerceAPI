# Rocket Ecommerce Templates

Rocket E-commerce has a number of templates that are expected and are hardcoded.



**MiniCart.cshtml**

This template is used when the "GetMiniCartDisplay()" is called, this can be called from the page directory by JS, injected into the razor  or called by the mini cart return in " AddToCart()".

                case "remote_minicart":
                    strOut = GetMiniCartDisplay();
                    break;
                case "remote_addtocart":
                    strOut = AddToCart();
                    break;