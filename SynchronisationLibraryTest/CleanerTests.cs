using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronisationLibrary;
//using WrapperLibrary;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace SynchronisationTest
{
    // TODO:: 
    // 1. use mock file/directory classes
    // 2. check whether events are being raised correctly
    // enable cleaner to be configured and scheduled through service
    // replace any refs to System.IO.Path.Combine with methods on wrapper ie constructor accepting Directory, Name

    [TestClass]
    public class CleanerTests
    {                
        [TestMethod]
        public void TestCleanWhenTargetPathIsNull()
        {            
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source");
            try
            {
                Cleaner.Clean(fileSystem, null, @"C:\Source");
                Assert.Fail("Exception not raised."); 
            }
            catch (ArgumentNullException)
            {           
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestCleanWhenSourcePathIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Target");
            try
            {
                Cleaner.Clean(fileSystem, @"C:\Target", null);
                Assert.Fail("Exception not raised.");
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestCleanWhenTargetIsNotFound()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source");
            try
            {
                Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source");
                Assert.Fail("Exception not raised.");
            }
            catch (SyncDirectoryNotFoundException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestCleanWhenSourceIsNotFound()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Target");
            try
            {
                Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source");
                Assert.Fail("Exception not raised.");
            }
            catch (SyncDirectoryNotFoundException)
            { }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestCleanByPattern()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source");
            fileSystem.AddFile(@"C:\Target\File.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File.doc", new MockFileData("Data"));
            
            // Run the test.
            Cleaner.Result result = Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source", "*.txt");

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeletedEntries);
            Assert.AreEqual(1, result.DeletedEntries.Count);
            Assert.AreEqual("File.txt", result.DeletedEntries[0]);
            Assert.IsNotNull(result.MatchedFiles);
            Assert.AreEqual(0, result.MatchedFiles.Count);
            Assert.IsNotNull(result.DirectoryResults);
            Assert.AreEqual(0, result.DirectoryResults.Count);
        }

       // //[TestMethod]
       // public void TestCleanByAttributes()
       // {
       //     // Prepare the source and target directories and files.
       //     IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
       //     IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
       //     IFile targetFile1 = Common.CreateFile("File1.txt", Common.TargetDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Normal);
       //     IFile targetFile2 = Common.CreateFile("File2.txt", Common.TargetDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Hidden);

       //     // Run the test.
       //     Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory, "*.*", FileAttributesWrapper.Normal);

       //     // Check the results
       //     Assert.IsNotNull(result);
       //     Assert.IsNotNull(result.DeletedEntries);
       //     Assert.AreEqual(1, result.DeletedEntries.Count);
       //     Assert.AreEqual(targetFile1.Name, result.DeletedEntries[0]);
       //     Assert.IsNotNull(result.MatchedFiles);
       //     Assert.AreEqual(0, result.MatchedFiles.Count);
       //     Assert.IsNotNull(result.SubDirectoryResults);
       //     Assert.AreEqual(0, result.SubDirectoryResults.Count);
       // }

       // //[TestMethod]
       // public void TestCleanByPatternAndAttributes()
       // {
       //     // Prepare the source and target directories and files.
       //     IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
       //     IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
       //     IFile targetFile1 = Common.CreateFile("File1.txt", Common.TargetDirectoryPath, new byte[] { 65 }, 
       //         FileAttributesWrapper.Hidden);
       //     IFile targetFile2 = Common.CreateFile("File2.doc", Common.TargetDirectoryPath, new byte[] { 65 }, 
       //         FileAttributesWrapper.Hidden);
       //     IFile targetFile3 = Common.CreateFile("File3.txt", Common.TargetDirectoryPath, new byte[] { 65 });

       //     // Run the test.
       //     Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory, "*.txt", FileAttributesWrapper.Hidden);

       //     // Check the results
       //     Assert.IsNotNull(result);
       //     Assert.IsNotNull(result.DeletedEntries);
       //     Assert.AreEqual(1, result.DeletedEntries.Count);
       //     Assert.AreEqual(targetFile1.Name, result.DeletedEntries[0]);
       //     Assert.IsNotNull(result.MatchedFiles);
       //     Assert.AreEqual(0, result.MatchedFiles.Count);
       //     Assert.IsNotNull(result.SubDirectoryResults);
       //     Assert.AreEqual(0, result.SubDirectoryResults.Count);
       // }
   
       // //[TestMethod]
       // public void TestClean()//TODO:: make recursive?
       // {
       //     // Prepare the source and target directories and files.
       //     IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
       //     IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
       //     IFile matchedSourceFile = Common.CreateFile("MatchedFile.txt", Common.SourceDirectoryPath, new byte[] { 65 });
       //     IFile matchedTargetFile = Common.CreateFile("MatchedFile.txt", Common.TargetDirectoryPath, new byte[] { 65 });
       //     IFile unmatchedTargetFile = Common.CreateFile("UnmatchedFile.txt", Common.TargetDirectoryPath, new byte[] { 65 });
       //     IDirectory unmatchedTargetSubDirectory = Common.CreateDirectory(Common.TargetSubDirectoryPath);

       //     // Run the test.
       //     Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory);

       //     // Check the results
       //     Assert.IsNotNull(result);
       //     Assert.IsNotNull(result.DeletedEntries);
       //     Assert.AreEqual(2, result.DeletedEntries.Count);
       //     Assert.IsTrue(result.DeletedEntries.Contains(unmatchedTargetFile.Name));
       //     Assert.IsTrue(result.DeletedEntries.Contains(unmatchedTargetSubDirectory.Name));
       //     Assert.IsNotNull(result.MatchedFiles);
       //     Assert.AreEqual(1, result.MatchedFiles.Count);
       //     Assert.AreEqual(matchedTargetFile.Name, result.MatchedFiles[0]);
       //     Assert.IsNotNull(result.SubDirectoryResults);
       //     Assert.AreEqual(0, result.SubDirectoryResults.Count);
       // }  

       // //[TestMethod]
       // public void TestCleanWhenSubDirectoriesMatch()
       // {
       //     // Prepare the source and target directories and files.
       //     IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
       //     IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
       //     IDirectory sourceSubDirectory = Common.CreateDirectory(Common.SourceSubDirectoryPath);
       //     IDirectory targetSubDirectory = Common.CreateDirectory(Common.TargetSubDirectoryPath);

       //     // Run the test.
       //     Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory);

       //     // Check the results
       //     Assert.IsNotNull(result);
       //     Assert.IsNotNull(result.DeletedEntries);
       //     Assert.AreEqual(0, result.DeletedEntries.Count);
       //     Assert.IsNotNull(result.MatchedFiles);
       //     Assert.AreEqual(0, result.MatchedFiles.Count);
       //     Assert.IsNotNull(result.SubDirectoryResults);
       //     Assert.AreEqual(1, result.SubDirectoryResults.Count);
       //     Assert.AreEqual(Common.SubDirectoryName, result.SubDirectoryResults.Keys.First());
       // }
    }
}
