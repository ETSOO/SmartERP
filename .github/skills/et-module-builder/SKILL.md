---
name: et-module-builder
description: Add new module files to the project following current examples.
---

# Table

- The table name to build: ${input:table:Table name} or take the first parameter from the command line. Please change the table name to camel case if the table name contains underscores. For example, 'product_unit' will be changed to 'ProductUnit'.
- Read table definition from directory 'Platform\Platform.Server\Models' and rebuild the definition in directory 'PlatformShared\Database\Models', also create the configration file. For example, table 'product_unit' in file 'ProductUnit.cs' will be read and the definition will be rebuilt with 'ProductUnit.cs', 'ProductUnitConfiguration.cs' under 'Configurations'.
- When the definition is not found, show a warning and let the user to input another table name or exit the command.
- Add the table to 'MyDbContext' and 'ApplyConfiguration' in 'OnModelCreating' method.

# Backend Project

- The project to build: ${input:project:Project name} or take the second parameter from the command line. Collect all the project folders under the solution and show in the dropdown list for user to select.
- The project should exist in the solution under directory '${project}\${project}.Server', otherwise show a warning and let the user to input another project name or exit the command.
- Create a service file and interface file in directory '${project}\${project}.Server\Services' with the name '${TableName}Service.cs' and add methods but replace the names like 'PromotionService.cs'. Also create subfolder with same naming style under 'Dto' and 'RQ' when creating other request data and response data types.
- Create a router file in directory '${project}\${project}.Server\Endpoints' with the name '${TableName}.cs' and add methods but replace the names like 'Promotion.cs'.
- Add the service and endpoint to 'Program.cs' with the same naming style like 'PromotionService'.

# Frontend API

- Create a 'temp' folder under directory '${project}\${project}.client\src'.
- Create a API file like ${table}Api.ts under the 'temp' folder and add API methods like 'PromotionApi.ts' in the skill's 'resources/ts' folder.
- Create all the necessary request data and response data files under the 'temp' folder with the same naming style. Create seperate TypeScript files in 'dto' for enum types.

# Frontend pages

## Confirm with the user to create the page files for the new module.

## If yes, continue to create the page files, otherwise exit the command.

- The name to configure the route: ${input:route:Route name} or take the third parameter from the command line. Collect all the first level route names under '/home' from 'main.tsx' and show in the dropdown list for user to select.
- The module pages to clone: ${input:modulePages:Module pages} or take the fourth parameter from the command line.
- Refer to all components under ${modulePages}, add similar components in the ${root} folder for the new module. Don't explore the i18n label definition and other details what are out of the current project, dev will manually add the labels in the language files.
- Add the route with name ${route} to 'main.tsx' and 'Layout.tsx' with the same naming style.
- Don't run the project to test, dev will manually test after all the files are created.
