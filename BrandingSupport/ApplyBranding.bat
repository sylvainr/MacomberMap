REM Apply our branding components to all Macomber Map areas
@ECHO OFF
COPY /Y CompanyLogo.* ..\Client\MacomberMacomberMap.UI\Resources
COPY /Y CompanyLogo.* ..\Client\MacomberMapAdministrationConsole\Resources
COPY /Y CompanyLogo.* ..\Client\MacomberMapClient\Resources


COPY /Y CompanyLogo.* ..\Server\MacomberMapAdministrationService\Resources
COPY /Y CompanyLogo.* ..\Server\MacomberMapIntegratedService\Resources

COPY /Y CompanyLogo.* ..\Support\CodeArchiveGenerator\Resources
COPY /Y CompanyLogo.* ..\Support\TEDESimulator\Resources

COPY /Y CompanyLogo.* ..\MacomberMapCommunications\Resources
