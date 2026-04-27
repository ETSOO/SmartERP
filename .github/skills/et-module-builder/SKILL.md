---
name: et-module-builder
description: Add new module files to the project following current examples.
---

# Table

- Question user to input which table to build from. Please change the table name to camel case if the table name contains underscores. For example, 'product_unit' will be changed to 'ProductUnit' as reference $TableName.
- Read table definition from directory 'Platform\Platform.Server\Models' and rebuild the definition in directory 'PlatformShared\Database\Models', also create the configration file. For example, table 'product_unit' in file 'ProductUnit.cs' will be read and the definition will be rebuilt with 'ProductUnit.cs', 'ProductUnitConfiguration.cs' under 'Configurations'.
- When the definition is not found, show a warning and let the user to input another table name or exit the command.
- Add the table to 'MyDbContext' and 'ApplyConfiguration' in 'OnModelCreating' method.

# New Module

- Question user to input which module to build as reference $NewModule, default is the $TableName.

# Source Module

- Question user to input which source module to copy / refer from as reference $SourceModule. Like 'Promotion' or 'Order'.

# Backend Project

- Question user which project to build from as reference $Project. Collect all the project folders under the solution and show in the dropdown list for user to select.
- The project should exist in the solution under directory '${Project}\${Project}.Server'. Copy all features from the source module $SourceModule.
- Create a service file and interface file in directory '${Project}\${Project}.Server\Services' with the name '${NewModule}Service.cs', then add all methods in the file '${SourceModule}Service.cs'. If no 'order_index' column, ignore the 'SortAsync' method. Also create subfolder with same naming style under 'Dto' and 'RQ' when creating other request data and response data types.
- Create a router file in directory '${Project}\${Project}.Server\Endpoints' with the name '${NewModule}.cs' and add methods but replace the names like 'Promotion.cs'.
- Add the service and endpoint to 'Program.cs' with the same naming style like 'PromotionService'.

# Frontend pages

## Question user whether to create the page files for the new module. If yes, continue below steps, otherwise exit the command.

- Question user what route to use as reference $Route. Collect all the first level route names under '/home' from 'main.tsx' and show in the dropdown list for user to select.
- Refer to all components under ${SourceModule}, add similar components in the ${root} folder for the new module. Don't explore the i18n label definition and other details what are out of the current project, dev will manually add the labels in the language files.
- Add the route with name ${Route} to 'main.tsx' and 'Layout.tsx' with the same naming style.
- Don't run the project to test, dev will manually test after all the files are created.
