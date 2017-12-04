@{
GUID="d565a442-a300-425a-a0d2-bde79b7da5ac"
Author="Microsoft Corporation"
CompanyName="Microsoft Corporation"
Copyright="Copyright (c) Microsoft Corporation. All rights reserved."
ModuleVersion="1.0.0.0"
PowerShellVersion="3.0"
CLRVersion="4.0"
NestedModules="Microsoft.PowerShell.WindowsManagement.dll"
HelpInfoURI = 'https://go.microsoft.com/fwlink/?linkid='
AliasesToExport = @("gcb", "scb")
FunctionsToExport = @()
TypesToProcess="GetEvent.types.ps1xml"
FormatsToProcess="Event.format.ps1xml","Diagnostics.format.ps1xml"
CmdletsToExport=@(
    "Get-EventLog",
    "Clear-EventLog",
    "Write-EventLog",
    "Limit-EventLog",
    "Show-EventLog",
    "New-EventLog",
    "Remove-EventLog",
    "Get-WmiObject",
    "Invoke-WmiMethod",
    "Remove-WmiObject",
    "Register-WmiEvent",
    "Set-WmiInstance",
    "Get-Transaction",
    "Start-Transaction",
    "Complete-Transaction",
    "Undo-Transaction",
    "Use-Transaction",
    "New-WebServiceProxy",
    "Get-HotFix",
    "Test-Connection",
    "Enable-ComputerRestore",
    "Disable-ComputerRestore",
    "Checkpoint-Computer",
    "Get-ComputerRestorePoint",
    "Test-ComputerSecureChannel",
    "Reset-ComputerMachinePassword",
    "Get-ControlPanelItem",
    "Show-ControlPanelItem",
    "Clear-Recyclebin",
    "Get-Clipboard",
    "Set-Clipboard",
    "Get-Counter", 
    "Import-Counter", 
    "Export-Counter"
    )
}
