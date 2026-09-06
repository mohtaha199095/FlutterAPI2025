FastReport Online Designer — required static files
=================================================

Copy the entire "WebReportDesigner" folder from your FastReport installation
(or download from https://www.fast-report.com/) into this directory so that:

  wwwroot/WebReportDesigner/index.html

exists.

Without these files, the ERP will still allow uploading .frx files from
Settings → Transaction Reports → Customize → Upload .frx.

For the desktop designer download button, also deploy:

  wwwroot/tools/FastReport.rar

(see wwwroot/tools/README.txt).
