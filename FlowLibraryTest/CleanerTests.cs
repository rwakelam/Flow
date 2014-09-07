using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowLibrary;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace FlowLibraryTest
{
    // TODO:: 
    // 1. check whether events are being raised correctly
    
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
            catch (SyncDirectoryNotFoundException)//TODO:: replace with straight System.IO.Exception?
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
            Cleaner.CleanResult result = Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source", "*.txt");

            // Check the clean has worked properly.
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\File.txt"));
            Assert.IsTrue(fileSystem.File.Exists(@"C:\Target\File.doc"));
        }

        [TestMethod]
        public void TestCleanByAttributes()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source");
            fileSystem.AddFile(@"C:\Target\File1.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File2.txt", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Target\File1.txt", FileAttributes.Normal);
            fileSystem.File.SetAttributes(@"C:\Target\File2.txt", FileAttributes.Hidden);

            // Run the test.
            Cleaner.CleanResult result = Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source", "*.*", FileAttributes.Normal);

            // Check the results
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\File1.txt"));
            Assert.IsTrue(fileSystem.File.Exists(@"C:\Target\File2.txt"));
        }
        
        [TestMethod]
        public void TestCleanByPatternAndAttributes()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source");
            fileSystem.AddFile(@"C:\Target\File1.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File2.doc", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File3.txt", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Target\File1.txt", FileAttributes.Normal);
            fileSystem.File.SetAttributes(@"C:\Target\File2.doc", FileAttributes.Normal);
            fileSystem.File.SetAttributes(@"C:\Target\File3.txt", FileAttributes.Hidden);

            // Run the test.
            Cleaner.CleanResult result = Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source", "*.txt", FileAttributes.Normal);

            // Check the results
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\File1.txt"));
            Assert.IsTrue(fileSystem.File.Exists(@"C:\Target\File2.doc"));
            Assert.IsTrue(fileSystem.File.Exists(@"C:\Target\File3.txt"));
        }

        [TestMethod]
        public void TestCleanRecursive()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source\SubDirectory");
            fileSystem.AddFile(@"C:\Target\SubDirectory\File.txt", new MockFileData("Data"));

            // Run the test.
            Cleaner.CleanResult result = Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source");

            // Check the results
            Assert.IsFalse(fileSystem.File.Exists(@"C:\Target\SubDirectory\File.txt"));
        }

        [TestMethod]              
        public void TestCleanResult()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File1.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File1.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File2.txt", new MockFileData("Data"));
            fileSystem.AddDirectory(@"C:\Source\SubDirectory");
            fileSystem.AddDirectory(@"C:\Target\SubDirectory");

            // Run the test.
            var result = Cleaner.Clean(fileSystem, @"C:\Target", @"C:\Source");

            // Check the result.
            Assert.AreEqual(1, result.MatchedFiles.Count);
            Assert.AreEqual("File1.txt", result.MatchedFiles[0]);
            Assert.AreEqual(1, result.DeletedEntries.Count);
            Assert.AreEqual("File2.txt", result.DeletedEntries[0]);
            Assert.AreEqual(0, result.FailedEntries.Count);
            // TODO:: figure out how to spoof a fail an entry?
            Assert.AreEqual(1, result.DirectoryResults.Count);
         }

    }
}
