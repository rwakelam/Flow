using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronisationLibrary;
using WrapperLibrary;

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
        public void TestCleanWhenTargetDirectoryIsNull()
        {
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            try
            {
                Cleaner.Clean(null, sourceDirectory);
                Assert.Fail("Required exception not raised."); 
            }
            catch (ArgumentNullException ex)
            {           
            }
        }

        [TestMethod]
        public void TestCleanWhenSourceDirectoryIsNull()
        {
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            try
            {
                Cleaner.Clean(targetDirectory, null);
                Assert.Fail("Required exception not raised.");
            }
            catch (ArgumentNullException ex)
            {
            }
        }

        [TestMethod]
        public void TestCleanWhenTargetDirectoryIsNotFound()
        {
            IDirectory targetDirectory = new DirectoryWrapper(Common.TargetDirectoryPath);
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            if (targetDirectory.Exists()) 
            {
                targetDirectory.Delete();
            }
            try
            {
                Cleaner.Clean(targetDirectory, sourceDirectory);
                Assert.Fail("Required exception not raised."); 
            }
            catch (SyncDirectoryNotFoundException ex)
            {           
            }
        }

        [TestMethod]
        public void TestCleanWhenSourceDirectoryIsNotFound()
        {
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IDirectory sourceDirectory = new DirectoryWrapper(Common.SourceDirectoryPath);
            if (sourceDirectory.Exists())
            {
                sourceDirectory.Delete();
            }
            try
            {
                Cleaner.Clean(targetDirectory, sourceDirectory);
                Assert.Fail();// how is this supposed to work?
            }
            catch (SyncDirectoryNotFoundException ex) 
            { }
        }

        [TestMethod]
        public void TestCleanByTargetFilePattern()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile targetFile1 = Common.CreateFile("File.txt", Common.TargetDirectoryPath, new byte[] { 65 });
            IFile targetFile2 = Common.CreateFile("File.doc", Common.TargetDirectoryPath, new byte[] { 65 });

            // Run the test.
            Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory, "*.txt");

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeletedEntries);
            Assert.AreEqual(1, result.DeletedEntries.Count);
            Assert.AreEqual(targetFile1.Name, result.DeletedEntries[0]);
            Assert.IsNotNull(result.MatchedFiles);
            Assert.AreEqual(0, result.MatchedFiles.Count);
            Assert.IsNotNull(result.SubDirectoryResults);
            Assert.AreEqual(0, result.SubDirectoryResults.Count);
        }

        [TestMethod]
        public void TestCleanByTargetFileAttributes()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile targetFile1 = Common.CreateFile("File1.txt", Common.TargetDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Normal);
            IFile targetFile2 = Common.CreateFile("File2.txt", Common.TargetDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Hidden);

            // Run the test.
            Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory, "*.*", FileAttributesWrapper.Normal);

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeletedEntries);
            Assert.AreEqual(1, result.DeletedEntries.Count);
            Assert.AreEqual(targetFile1.Name, result.DeletedEntries[0]);
            Assert.IsNotNull(result.MatchedFiles);
            Assert.AreEqual(0, result.MatchedFiles.Count);
            Assert.IsNotNull(result.SubDirectoryResults);
            Assert.AreEqual(0, result.SubDirectoryResults.Count);
        }

        [TestMethod]
        public void TestCleanByTargetFilePatternAndTargetFileAttributes()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile targetFile1 = Common.CreateFile("File1.txt", Common.TargetDirectoryPath, new byte[] { 65 }, 
                FileAttributesWrapper.Hidden);
            IFile targetFile2 = Common.CreateFile("File2.doc", Common.TargetDirectoryPath, new byte[] { 65 }, 
                FileAttributesWrapper.Hidden);
            IFile targetFile3 = Common.CreateFile("File3.txt", Common.TargetDirectoryPath, new byte[] { 65 });

            // Run the test.
            Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory, "*.txt", FileAttributesWrapper.Hidden);

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeletedEntries);
            Assert.AreEqual(1, result.DeletedEntries.Count);
            Assert.AreEqual(targetFile1.Name, result.DeletedEntries[0]);
            Assert.IsNotNull(result.MatchedFiles);
            Assert.AreEqual(0, result.MatchedFiles.Count);
            Assert.IsNotNull(result.SubDirectoryResults);
            Assert.AreEqual(0, result.SubDirectoryResults.Count);
        }
   
        [TestMethod]
        public void TestClean()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IFile matchedSourceFile = Common.CreateFile("MatchedFile.txt", Common.SourceDirectoryPath, new byte[] { 65 });
            IFile matchedTargetFile = Common.CreateFile("MatchedFile.txt", Common.TargetDirectoryPath, new byte[] { 65 });
            IFile unmatchedTargetFile = Common.CreateFile("UnmatchedFile.txt", Common.TargetDirectoryPath, new byte[] { 65 });
            IDirectory unmatchedTargetSubDirectory = Common.CreateDirectory(Common.TargetSubDirectoryPath);

            // Run the test.
            Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory);

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeletedEntries);
            Assert.AreEqual(2, result.DeletedEntries.Count);
            Assert.IsTrue(result.DeletedEntries.Contains(unmatchedTargetFile.Name));
            Assert.IsTrue(result.DeletedEntries.Contains(unmatchedTargetSubDirectory.Name));
            Assert.IsNotNull(result.MatchedFiles);
            Assert.AreEqual(1, result.MatchedFiles.Count);
            Assert.AreEqual(matchedTargetFile.Name, result.MatchedFiles[0]);
            Assert.IsNotNull(result.SubDirectoryResults);
            Assert.AreEqual(0, result.SubDirectoryResults.Count);
        }  

        [TestMethod]
        public void TestCleanWhenSubDirectoriesMatch()
        {
            // Prepare the source and target directories and files.
            IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
            IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
            IDirectory sourceSubDirectory = Common.CreateDirectory(Common.SourceSubDirectoryPath);
            IDirectory targetSubDirectory = Common.CreateDirectory(Common.TargetSubDirectoryPath);

            // Run the test.
            Cleaner.Result result = Cleaner.Clean(targetDirectory, sourceDirectory);

            // Check the results
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DeletedEntries);
            Assert.AreEqual(0, result.DeletedEntries.Count);
            Assert.IsNotNull(result.MatchedFiles);
            Assert.AreEqual(0, result.MatchedFiles.Count);
            Assert.IsNotNull(result.SubDirectoryResults);
            Assert.AreEqual(1, result.SubDirectoryResults.Count);
            Assert.AreEqual(Common.SubDirectoryName, result.SubDirectoryResults.Keys.First());
        }
    }
}
