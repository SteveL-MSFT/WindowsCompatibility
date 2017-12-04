/********************************************************************++
Copyright (c) Microsoft Corporation. All rights reserved.
--********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.PowerShell.Commands.Internal;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System.Linq;
using Dbg = System.Diagnostics;
using Microsoft.PowerShell.CoreClr.Stubs;

namespace Microsoft.PowerShell.Commands
{
#region Helper
    /// <summary>
    /// Helper Class used by Stop-Computer,Restart-Computer and Test-Connection
    /// Also Contain constants used by System Restore related Cmdlets.
    /// </summary>
    internal static class ComputerWMIHelper
    {
        /// <summary>
        /// The maximum length of a valid NetBIOS name
        /// </summary>
        internal const int NetBIOSNameMaxLength = 15;

        /// <summary>
        /// System Restore Class used by Cmdlets
        /// </summary>
        internal const string WMI_Class_SystemRestore = "SystemRestore";

        /// <summary>
        /// OperatingSystem WMI class used by Cmdlets
        /// </summary>
        internal const string WMI_Class_OperatingSystem = "Win32_OperatingSystem";

        /// <summary>
        /// Service WMI class used by Cmdlets
        /// </summary>
        internal const string WMI_Class_Service = "Win32_Service";

        /// <summary>
        /// Win32_ComputerSystem WMI class used by Cmdlets
        /// </summary>
        internal const string WMI_Class_ComputerSystem = "Win32_ComputerSystem";

        /// <summary>
        /// Ping Class used by Cmdlet.
        /// </summary>
        internal const string WMI_Class_PingStatus = "Win32_PingStatus";

        /// <summary>
        /// CIMV2 path
        /// </summary>
        internal const string WMI_Path_CIM = "\\root\\cimv2";

        /// <summary>
        /// Default path
        /// </summary>
        internal const string WMI_Path_Default = "\\root\\default";

        /// <summary>
        /// The error says The interface is unknown.
        /// </summary>
        internal const int ErrorCode_Interface = 1717;

        /// <summary>
        /// This error says An instance of the service is already running.
        /// </summary>
        internal const int ErrorCode_Service = 1056;

        /// <summary>
        /// The name of the privilege to shutdown a local system
        /// </summary>
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        /// <summary>
        /// The name of the privilege to shutdown a remote system
        /// </summary>
        internal const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";

        /// <summary>
        /// CimUriPrefix
        /// </summary>
        internal const string CimUriPrefix = "http://schemas.microsoft.com/wbem/wsman/1/wmi/root/cimv2";

        /// <summary>
        /// CimOperatingSystemNamespace
        /// </summary>
        internal const string CimOperatingSystemNamespace = "root/cimv2";

        /// <summary>
        /// CimOperatingSystemShutdownMethod
        /// </summary>
        internal const string CimOperatingSystemShutdownMethod = "Win32shutdown";

        /// <summary>
        /// CimQueryDialect
        /// </summary>
        internal const string CimQueryDialect = "WQL";

        /// <summary>
        /// Local host name
        /// </summary>
        internal const string localhostStr = "localhost";


        /// <summary>
        /// Get the local admin user name from a local NetworkCredential
        /// </summary>
        /// <param name="computerName"></param>
        /// <param name="psLocalCredential"></param>
        /// <returns></returns>
        internal static string GetLocalAdminUserName(string computerName, PSCredential psLocalCredential)
        {
            string localUserName = null;

            // The format of local admin username should be "ComputerName\AdminName"
            if (psLocalCredential.UserName.Contains("\\"))
            {
                localUserName = psLocalCredential.UserName;
            }
            else
            {
                int dotIndex = computerName.IndexOf(".", StringComparison.OrdinalIgnoreCase);
                if (dotIndex == -1)
                {
                    localUserName = computerName + "\\" + psLocalCredential.UserName;
                }
                else
                {
                    localUserName = computerName.Substring(0, dotIndex) + "\\" + psLocalCredential.UserName;
                }
            }

            return localUserName;
        }

        /// <summary>
        /// Generate a random password
        /// </summary>
        /// <param name="passwordLength"></param>
        /// <returns></returns>
        internal static string GetRandomPassword(int passwordLength)
        {
            const int charMin = 32, charMax = 122;
            const int allowedCharsCount = charMax - charMin + 1;
            byte[] randomBytes = new byte[passwordLength];
            char[] chars = new char[passwordLength];

            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            for (int i = 0; i < passwordLength; i++)
            {
                chars[i] = (char)(randomBytes[i] % allowedCharsCount + charMin);
            }

            return new string(chars);
        }

        /// <summary>
        /// Get the Connection Options
        /// </summary>
        /// <param name="Authentication"></param>
        /// <param name="Impersonation"></param>
        /// <param name="Credential"></param>
        /// <returns></returns>
        internal static ConnectionOptions GetConnectionOptions(AuthenticationLevel Authentication, ImpersonationLevel Impersonation, PSCredential Credential)
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Authentication = Authentication;
            options.EnablePrivileges = true;
            options.Impersonation = Impersonation;
            if (Credential != null)
            {
                options.Username = Credential.UserName;
                options.SecurePassword = Credential.Password;
            }
            return options;
        }

        /// <summary>
        /// Gets the Scope
        ///
        /// </summary>
        /// <param name="computer"></param>
        /// <param name="namespaceParameter"></param>
        /// <returns></returns>
        internal static string GetScopeString(string computer, string namespaceParameter)
        {
            StringBuilder returnValue = new StringBuilder("\\\\");
            if (computer.Equals("::1", StringComparison.CurrentCultureIgnoreCase) || computer.Equals("[::1]", StringComparison.CurrentCultureIgnoreCase))
            {
                returnValue.Append("localhost");
            }
            else
            {
                returnValue.Append(computer);
            }
            returnValue.Append(namespaceParameter);
            return returnValue.ToString();
        }

        /// <summary>
        /// Returns true if it is a valid drive on the system.
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        internal static bool IsValidDrive(string drive)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo logicalDrive in drives)
            {
                if (logicalDrive.DriveType.Equals(DriveType.Fixed))
                {
                    if (drive.ToString().Equals(logicalDrive.Name.ToString(), System.StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether string[] contains System Drive.
        /// </summary>
        /// <param name="drives"></param>
        /// <param name="sysdrive"></param>
        /// <returns></returns>
        internal static bool ContainsSystemDrive(string[] drives, string sysdrive)
        {
            string driveApp;
            foreach (string drive in drives)
            {
                if (!drive.EndsWith("\\", StringComparison.CurrentCultureIgnoreCase))
                {
                    driveApp = String.Concat(drive, "\\");
                }
                else
                    driveApp = drive;
                if (driveApp.Equals(sysdrive, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the given computernames in a string
        /// </summary>
        /// <param name="computerNames"></param>
        internal static string GetMachineNames(string[] computerNames)
        {
            string separator = ",";
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\International");
            if (regKey != null)
            {
                object sListValue = regKey.GetValue("sList");
                if (sListValue != null)
                {
                    separator = sListValue.ToString();
                }
            }

            string compname = string.Empty;
            StringBuilder strComputers = new StringBuilder();
            int i = 0;
            foreach (string computer in computerNames)
            {
                if (i > 0)
                {
                    strComputers.Append(separator);
                }
                else
                {
                    i++;
                }

                if ((computer.Equals("localhost", StringComparison.CurrentCultureIgnoreCase)) || (computer.Equals(".", StringComparison.OrdinalIgnoreCase)))
                {
                    compname = Dns.GetHostName();
                }
                else
                {
                    compname = computer;
                }

                strComputers.Append(compname);
            }

            return strComputers.ToString();
        }

        internal static ComputerChangeInfo GetComputerStatusObject(int errorcode, string computername)
        {
            ComputerChangeInfo computerchangeinfo = new ComputerChangeInfo();
            computerchangeinfo.ComputerName = computername;
            if (errorcode != 0)
            {
                computerchangeinfo.HasSucceeded = false;
            }
            else
            {
                computerchangeinfo.HasSucceeded = true;
            }
            return computerchangeinfo;
        }

        internal static void WriteNonTerminatingError(int errorcode, PSCmdlet cmdlet, string computername)
        {
            Win32Exception ex = new Win32Exception(errorcode);
            string additionalmessage = String.Empty;
            if (ex.NativeErrorCode.Equals(0x00000035))
            {
                additionalmessage = StringUtil.Format(ComputerResources.NetworkPathNotFound, computername);
            }
            string message = StringUtil.Format(ComputerResources.OperationFailed, ex.Message, computername, additionalmessage);
            ErrorRecord er = new ErrorRecord(new InvalidOperationException(message), "InvalidOperationException", ErrorCategory.InvalidOperation, computername);
            cmdlet.WriteError(er);
        }

        /// <summary>
        /// Check whether the new computer name is valid
        /// </summary>
        /// <param name="computerName"></param>
        /// <returns></returns>
        internal static bool IsComputerNameValid(string computerName)
        {
            bool allDigits = true;

            if (computerName.Length >= 64)
                return false;

            foreach (char t in computerName)
            {
                if (t >= 'A' && t <= 'Z' ||
                    t >= 'a' && t <= 'z')
                {
                    allDigits = false;
                    continue;
                }
                else if (t >= '0' && t <= '9')
                {
                    continue;
                }
                else if (t == '-')
                {
                    allDigits = false;
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return !allDigits;
        }

        /// <summary>
        /// System Restore APIs are not supported on the ARM platform. Skip the system restore operation is necessary.
        /// </summary>
        /// <param name="cmdlet"></param>
        /// <returns></returns>
        internal static bool SkipSystemRestoreOperationForARMPlatform(PSCmdlet cmdlet)
        {
            bool retValue = false;
            if (PsUtils.IsRunningOnProcessorArchitectureARM())
            {
                var ex = new InvalidOperationException(ComputerResources.SystemRestoreNotSupported);
                var er = new ErrorRecord(ex, "SystemRestoreNotSupported", ErrorCategory.InvalidOperation, null);
                cmdlet.WriteError(er);
                retValue = true;
            }
            return retValue;
        }

        /// <summary>
        /// Invokes the Win32Shutdown command on provided target computer using WSMan
        /// over a CIMSession.  The flags parameter determines the type of shutdown operation
        /// such as shutdown, reboot, force etc.
        /// </summary>
        /// <param name="cmdlet">Cmdlet host for reporting errors</param>
        /// <param name="isLocalhost">True if local host computer</param>
        /// <param name="computerName">Target computer</param>
        /// <param name="flags">Win32Shutdown flags</param>
        /// <param name="credential">Optional credential</param>
        /// <param name="authentication">Optional authentication</param>
        /// <param name="formatErrorMessage">Error message format string that takes two parameters</param>
        /// <param name="ErrorFQEID">Fully qualified error Id</param>
        /// <param name="cancelToken">Cancel token</param>
        /// <returns>True on success</returns>
        internal static bool InvokeWin32ShutdownUsingWsman(
            PSCmdlet cmdlet,
            bool isLocalhost,
            string computerName,
            object[] flags,
            PSCredential credential,
            string authentication,
            string formatErrorMessage,
            string ErrorFQEID,
            CancellationToken cancelToken)
        {
            Dbg.Diagnostics.Assert(flags.Length == 2, "Caller need to verify the flags passed in");

            bool isSuccess = false;
            string targetMachine = isLocalhost ? "localhost" : computerName;
            string authInUse = isLocalhost ? null : authentication;
            PSCredential credInUse = isLocalhost ? null : credential;
            var currentPrivilegeState = new PlatformInvokes.TOKEN_PRIVILEGE();
            var operationOptions = new CimOperationOptions
            {
                Timeout = TimeSpan.FromMilliseconds(10000),
                CancellationToken = cancelToken,
                //This prefix works against all versions of the WinRM server stack, both win8 and win7
                ResourceUriPrefix = new Uri(ComputerWMIHelper.CimUriPrefix)
            };

            try
            {
                if (!(isLocalhost && PlatformInvokes.EnableTokenPrivilege(ComputerWMIHelper.SE_SHUTDOWN_NAME, ref currentPrivilegeState)) &&
                    !(!isLocalhost && PlatformInvokes.EnableTokenPrivilege(ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME, ref currentPrivilegeState)))
                {
                    string message =
                        StringUtil.Format(ComputerResources.PrivilegeNotEnabled, computerName,
                            isLocalhost ? ComputerWMIHelper.SE_SHUTDOWN_NAME : ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME);
                    ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(message), "PrivilegeNotEnabled", ErrorCategory.InvalidOperation, null);
                    cmdlet.WriteError(errorRecord);
                    return false;
                }

                using (CimSession cimSession = RemoteDiscoveryHelper.CreateCimSession(targetMachine, credInUse, authInUse, cancelToken, cmdlet))
                {
                    var methodParameters = new CimMethodParametersCollection();
                    int retVal;
                    methodParameters.Add(CimMethodParameter.Create(
                        "Flags",
                        flags[0],
                        Microsoft.Management.Infrastructure.CimType.SInt32,
                        CimFlags.None));

                    methodParameters.Add(CimMethodParameter.Create(
                        "Reserved",
                        flags[1],
                        Microsoft.Management.Infrastructure.CimType.SInt32,
                        CimFlags.None));

                    if ( ! InternalTestHooks.TestStopComputer ) 
                    {
                        CimMethodResult result = cimSession.InvokeMethod(
                            ComputerWMIHelper.CimOperatingSystemNamespace,
                            ComputerWMIHelper.WMI_Class_OperatingSystem,
                            ComputerWMIHelper.CimOperatingSystemShutdownMethod,
                            methodParameters,
                            operationOptions);

                        retVal = Convert.ToInt32(result.ReturnValue.Value, CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        retVal = InternalTestHooks.TestStopComputerResults;
                    }

                    if (retVal != 0)
                    {
                        var ex = new Win32Exception(retVal);
                        string errMsg = StringUtil.Format(formatErrorMessage, computerName, ex.Message);
                        ErrorRecord error = new ErrorRecord(
                            new InvalidOperationException(errMsg), ErrorFQEID, ErrorCategory.OperationStopped, computerName);
                        cmdlet.WriteError(error);
                    }
                    else
                    {
                        isSuccess = true;
                    }
                }
            }
            catch (CimException ex)
            {
                string errMsg = StringUtil.Format(formatErrorMessage, computerName, ex.Message);
                ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), ErrorFQEID,
                                                    ErrorCategory.OperationStopped, computerName);
                cmdlet.WriteError(error);
            }
            catch (Exception ex)
            {
                string errMsg = StringUtil.Format(formatErrorMessage, computerName, ex.Message);
                ErrorRecord error = new ErrorRecord(new InvalidOperationException(errMsg), ErrorFQEID,
                                                    ErrorCategory.OperationStopped, computerName);
                cmdlet.WriteError(error);
            }
            finally
            {
                // Restore the previous privilege state if something unexpected happened
                PlatformInvokes.RestoreTokenPrivilege(
                    isLocalhost ? ComputerWMIHelper.SE_SHUTDOWN_NAME : ComputerWMIHelper.SE_REMOTE_SHUTDOWN_NAME, ref currentPrivilegeState);
            }

            return isSuccess;
        }

        /// <summary>
        /// Returns valid computer name or null on failure.
        /// </summary>
        /// <param name="nameToCheck">Computer name to validate</param>
        /// <param name="shortLocalMachineName"></param>
        /// <param name="fullLocalMachineName"></param>
        /// <param name="error"></param>
        /// <returns>Valid computer name</returns>
        internal static string ValidateComputerName(
            string nameToCheck,
            string shortLocalMachineName,
            string fullLocalMachineName,
            ref ErrorRecord error)
        {
            string validatedComputerName = null;

            if (nameToCheck.Equals(".", StringComparison.OrdinalIgnoreCase) ||
                nameToCheck.Equals(localhostStr, StringComparison.OrdinalIgnoreCase) ||
                nameToCheck.Equals(shortLocalMachineName, StringComparison.OrdinalIgnoreCase) ||
                nameToCheck.Equals(fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
            {
                validatedComputerName = localhostStr;
            }
            else
            {
                bool isIPAddress = false;
                try
                {
                    IPAddress unused;
                    isIPAddress = IPAddress.TryParse(nameToCheck, out unused);
                }
                catch (Exception)
                {
                }

                try
                {
                    string fqcn = Dns.GetHostEntryAsync(nameToCheck).Result.HostName;
                    if (fqcn.Equals(shortLocalMachineName, StringComparison.OrdinalIgnoreCase) ||
                        fqcn.Equals(fullLocalMachineName, StringComparison.OrdinalIgnoreCase))
                    {
                        // The IPv4 or IPv6 of the local machine is specified
                        validatedComputerName = localhostStr;
                    }
                    else
                    {
                        validatedComputerName = nameToCheck;
                    }
                }
                catch (Exception e)
                {
                    // If GetHostEntry() throw exception, then the target should not be the local machine
                    if (!isIPAddress)
                    {
                        // Return error if the computer name is not an IP address. Dns.GetHostEntry() may not work on IP addresses.
                        string errMsg = StringUtil.Format(ComputerResources.CannotResolveComputerName, nameToCheck, e.Message);
                        error = new ErrorRecord(
                            new InvalidOperationException(errMsg), "AddressResolutionException",
                            ErrorCategory.InvalidArgument, nameToCheck);

                        return null;
                    }
                    validatedComputerName = nameToCheck;
                }
            }

            return validatedComputerName;
        }
    }
#endregion Helper

}//End namespace
