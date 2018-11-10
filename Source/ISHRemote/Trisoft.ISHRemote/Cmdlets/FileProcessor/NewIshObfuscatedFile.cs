/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Management.Automation;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.FileProcessor
{
    /// <summary>
    /// <para type="synopsis">The New-IshObfuscatedFile cmdlet can be used to obfuscate the text in xml files or replace images with a generated image of the same format, width and height.</para>
    /// <para type="description">
    /// The New-IshObfuscatedFile cmdlet can be used to:
    /// 1. For all files having a file extension that is present in XmlFileExtensions:
    ///   All the words in the text of all xml files are replaced with hardcoded words of the same length. 
    ///   This includes the text in elements, the comments and the processing instructions. 
    ///   Optionally, you can specifiy the names of attributes in the XmlAttributesToObfuscate parameter for which the values need to be obfuscated as well. 
    ///   By default, attribute values are not obfuscated, as some attribute values are limited to a certain list of values defined in the DTD. 
    ///   Note that an obfuscated xml is not validated against it's DTD or schema.
    /// 2. For all files having a file extension that is present in ImageFileExtensions
    ///   A new image is created with the same format, width and height as the original image. The new image will have a yellow background 
    ///   (and the filename in text in the image if it is wide enough to put it there).
    /// 3. For all files NOT having a file extension that is present in XmlImageExtensions or ImageFileExtensions
    ///   A warning message is given on the console that the file could not be obfuscated  
    ///   
    /// It will do that for all files that are passed through the pipeline or via the -FilePath parameter. 
    /// 
    /// Only the successfully obfuscated files will be saved to the output folder with the same name, overwriting the file with the same name if that is already present
    /// When the obfuscation fails or if there is no obfuscator for the given file extension, a warning message is given on the console that the file could not be obfuscated  
    /// 
    /// You can specify which file extensions should be treated as xml, by specifying the XmlFileExtensions parameter. By default these are ".xml", ".dita", ".ditamap".
    /// You can specify which file extensions should be treated as image, by specifying the XmlFileExtensions parameter. By default these are ".png", ".gif", ".bmp", ".jpg", ".jpeg".
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// # This sample script will process all files in the input folder and save the obfuscated xml and image files in the output folder
    /// # Other type of files like .mpg, .mov, .swf etc. are not supported by the obfuscator, so will not be present in the output folder
    /// Note that the script deletes all the files in the output folder first
    /// 
    /// Write-Host "Setting current directory..."
    /// [string]$currentScriptDirectory = split-path -parent $MyInvocation.MyCommand.Definition
    /// Set-Location $currentScriptDirectory
    /// [Environment]::CurrentDirectory=$currentScriptDirectory
    /// # Input folder with the files to obfuscate
    /// $inputfolder = "Data-ObfuscateFiles\InputFiles"	
    /// # Output folder with the obfuscated files.
    /// $outputfolder = "Data-ObfuscateFiles\OutputFiles"
    /// 
    /// # Clear the output folder
    /// if (Test-Path $outputfolder)
    /// {
    /// 	Remove-Item $outputfolder -Recurse
    /// }
    /// 
    /// # Read all files to process from the inputfolder
    /// $filesToProcess = get-childItem inputfolder -File
    /// 
    /// # If you also want to obfuscate certain attribute values when obfuscating an xml file, specify their attribute names here to obfuscate them as well. 
    /// # Important: If you include attribute names of which the values adheres to a fixed list list of values in the DTD, the file will not be valid anymore against the DTD.
    /// $attributesToObfuscate = @("navtitle")
    /// # Obfuscates all xml and image files in the folder
    /// # The successfully obfuscated files will be saved in the output folder
    /// Write-Host "Starting obfuscation process"
    /// $result = $filesToProcess | New-IshObfuscatedFile -OutputFolder $outputfolder -XmlAttributesToObfuscate $attributesToObfuscate
    /// Write-Host "Done obfuscation $($result.Length) of $($filesToProcess.Length) file obfuscated"
    /// </code>
    /// <para>Obfuscates all xml and image files in a certain directory</para>
    /// </example>
    /// <example>
    /// <code>
    /// $samplefile = "C:\\temp\\test.html";
    /// $attributesToObfuscate = @();          # @() = empty array
    /// $xmlFileExtensions = @(".html")        # Since we know that the .html file contains xhtml which can be loaded as xml, add it to the file extensions that should be treated as xml
    ///     
    /// New-IshObfuscatedFile -OutputFolder "c:\\out\\" -FilePath $samplefile -XmlFileExtensions $xmlFileExtensions
    /// </code>
    /// <para>Obfuscates one xhtml file</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "IshObfuscatedFile", SupportsShouldProcess = false)]
    [OutputType(typeof(FileInfo))]
    public sealed class NewIshObfuscatedFile : FileProcessorCmdlet
    {
        /// <summary>
        /// <para type="description">FilePath can be used to specify one or more input xml file location.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string[] FilePath { get; set; }

        /// <summary>
        /// <para type="description">Folder on the Windows filesystem where to store the new data files</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty]
        public string FolderPath { get; set; }

        /// <summary>
        /// <para type="description">The file extensions that should be treated as an xml.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string[] XmlFileExtensions
        {
            get { return _xmlFileExtensions.ToArray(); }
            set { _xmlFileExtensions = new List<string>(value); }
        }

        /// <summary>
        /// <para type="description">The file extensions that should be treated as an image.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string[] ImageFileExtensions
        {
            get { return _imageFileExtensions.ToArray(); }
            set { _imageFileExtensions = new List<string>(value); }
        }

        /// <summary>
        /// <para type="description">Array of attribute names for which the values need to be obfuscated as well.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNull]
        public string[] XmlAttributesToObfuscate
        {
            get { return _xmlAttributesToObfuscate.ToArray(); }
            set { _xmlAttributesToObfuscate = new List<string>(value); }
        }


        #region Private fields
        /// <summary>
        /// Private field to store the file extensions that should be treated as an xml
        /// </summary>
        private List<string> _xmlFileExtensions = new List<string> { ".xml", ".dita", ".ditamap" };
        /// <summary>
        /// Private field to store the file extensions that should be treated as an image
        /// </summary>
        private List<string> _imageFileExtensions = new List<string> { ".png", ".gif", ".bmp", ".jpg", ".jpeg" };
        /// <summary>
        /// Private field to store the file extensions that should be treated as an image
        /// </summary>
        private List<string> _xmlAttributesToObfuscate = new List<string>();
        /// <summary>
        /// Collection of the files to process
        /// </summary>
        private readonly List<FileInfo> _files = new List<FileInfo>();
        #endregion


        protected override void BeginProcessing()
        {
            if (!Directory.Exists(FolderPath))
            {
                WriteDebug("Creating FolderPath[${FolderPath}]");
                string tempLocation = Directory.CreateDirectory(FolderPath).FullName;
            }
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                foreach (string filePath in FilePath)
                {
                    _files.Add(new FileInfo(filePath));
                }
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="System.IO.FileInfo"/> to the pipeline.</remarks>
        protected override void EndProcessing()
        {
            try
            {
                int current = 0;
                WriteDebug("Obfuscating _files.Count["+_files.Count+"]");
                foreach (FileInfo inputFile in _files)
                {
                    string outputFilePath = Path.Combine(FolderPath, inputFile.Name);
                    WriteDebug("Obfuscating inputFile[" + inputFile.FullName + "] to outputFile[" + outputFilePath + "]");
                    WriteParentProgress("Obfuscating inputFile[" + inputFile.FullName + "] to outputFile[" + outputFilePath + "]", ++current, _files.Count);
                    try
                    {
                        if (_xmlFileExtensions.Contains(inputFile.Extension))
                        {
                            IshObfuscator.ObfuscateXml(inputFile.FullName, outputFilePath, _xmlAttributesToObfuscate);
                            WriteObject(new FileInfo(outputFilePath));
                        }
                        else if (_imageFileExtensions.Contains(inputFile.Extension))
                        {
                            IshObfuscator.ObfuscateImage(inputFile.FullName, outputFilePath);
                            WriteObject(new FileInfo(outputFilePath));
                        }
                        else
                        {
                            WriteVerbose("Obfuscating inputFile[" + inputFile.FullName + "] to outputFile[" + outputFilePath + "] failed: The file extension is not in the list of xml or image file extensions.");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteWarning("Obfuscating inputFile[" + inputFile.FullName + "] to outputFile[" + outputFilePath + "] failed: " + ex.Message);
                    }

                }
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            finally
            {
                base.EndProcessing();
            }
        }
    }
}
