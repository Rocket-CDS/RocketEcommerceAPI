# Create RE "Action Provider"

## Intro
An Action provider is an assembly that can do operations on the system, without the need for a UI.

It can be executed from the RocketEcommerceAPI Portal Admin. Using the Assembly Name and The Namespace.classname.


## Build

1. Create new Project in VS using .Net Standard Class library.
2. Select "\DesktopModules\DNNrocketModules" folder. (optional)
3. Add Project "DNNrocketAPI" and Set Reference to it.
4. Add Project "RocketEcommerceAPI" and Set Reference to it.
5. Rename Class
6. Inherit ActionProvider and implement interface methods.

```
using RocketEcommerceAPI.Components;
using System;

namespace REexport
{
    public class ExportData : ActionProvider
    {
        public override string DoAction(PortalShopLimpet portalShop, string actionData)
        {
            // DO WORK
            return "";
        }
    }
}

```

Any return data will be displayed in the "completedmodelmsgreturn" popup element.

This popupp is hidden by default, so if a message is required on completion some JS must be added to the return data.

```
<script>
    $(document).ready(function () {
        $('#completedmodelmsgreturn').show();
    });
</script>
```


NOTE: If using DNNpackager, add command to the after build event in VS.
```
DNNpackager.exe $(ProjectDir) $(ProjectDir)$(OutDir) $(ConfigurationName)
```