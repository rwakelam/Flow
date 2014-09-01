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
    [TestClass]
    public class PusherTests
    {
        [TestMethod]
        public void TestPushDirectoryWhenSourcePathIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Target");
            try
            {
                Pusher.PushDirectory(fileSystem, null, @"C:\Target");
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
        public void TestPushDirectoryWhenTargetPathIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Source");
            try
            {
                Pusher.PushDirectory(fileSystem, @"C:\Source", null);
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
        public void TestPushDirectoryWhenSourceIsNotFound()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddDirectory(@"C:\Target");
            try
            {
                Pusher.PushDirectory(fileSystem, @"C:\Source", @"C:\Target");
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
        public void TestPushFileWhenSourceIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Target\File.txt", new MockFileData("Data")); 
            try
            {
                Pusher.PushFile(fileSystem, null, @"C:\Target\File.txt");
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
        public void TestPushFileWhenTargetIsNull()
        {
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data"));
            try
            {
                Pusher.PushFile(fileSystem, @"C:\Source\File.txt", null);
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
        public void TestPushFileWhenSourceIsNotFound()
        {            
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            try
            {
                Pusher.PushFile(fileSystem, @"C:\Source\File.txt", @"C:\Target\File.txt");
                Assert.Fail("Exception not raised.");
            }
            catch (SyncFileNotFoundException)
            {
            }
            catch
            {
                Assert.Fail("Wrong exception raised.");
            }
        }

        [TestMethod]
        public void TestPushFileWhenBytesNotEqual()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data2"));
            fileSystem.AddFile(@"C:\Target\File.txt", new MockFileData("Data"));
            Pusher.PushEntryResult result = Pusher.PushFile(fileSystem,
                @"C:\Source\File.txt", @"C:\Target\File.txt");
            Assert.AreEqual("Data2", fileSystem.File.ReadAllText(@"C:\Target\File.txt"));
        }

        [TestMethod]
        public void TestPushFileWhenAttributesNotEqual()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data"));
            fileSystem.AddFile(@"C:\Target\File.txt", new MockFileData("Data"));
            fileSystem.File.SetAttributes(@"C:\Source\File.txt", FileAttributes.Hidden);
            fileSystem.File.SetAttributes(@"C:\Target\File.txt", FileAttributes.Normal);
            Pusher.PushEntryResult result = Pusher.PushFile(fileSystem,
                @"C:\Source\File.txt", @"C:\Target\File.txt");
            Assert.AreEqual(FileAttributes.Hidden, 
                fileSystem.File.GetAttributes(@"C:\Target\File.txt"));
        }

        [TestMethod]
        public void TestPushNewFile()
        {
            // Prepare the source and target directories and files.
            var fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(@"C:\Source\File.txt", new MockFileData("Data"));
            Pusher.PushEntryResult result = Pusher.PushFile(fileSystem, @"C:\Source\File.txt",
                @"C:\Target\File.txt");
            Assert.AreEqual("Data", fileSystem.File.ReadAllText(@"C:\Target\File.txt"));
        }

        //[TestMethod]
        //public void TestPushFileWhenTargetMatchesSource()
        //{
        //    // Prepare the source and target directories and files.
        //    IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
        //    IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
        //    IFile sourceFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 });
        //    IFile targetFile = Common.CreateFile("File.txt", Common.TargetDirectoryPath, new byte[] { 65 });
        //    Pusher.PushEntryResult result = Pusher.Push(sourceFile, targetFile);
        //    Assert.AreEqual(Pusher.PushEntryResult.Verified, result);
        //}

        //[TestMethod]
        //public void TestPushDirectoryByPattern()
        //{
        //    // Prepare the source and target directories and files.
        //    IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
        //    IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
        //    IFile soughtFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 });
        //    IFile unsoughtFile = Common.CreateFile("File.doc", Common.SourceDirectoryPath, new byte[] { 65 });

        //    // Run the test.
        //    Pusher.PushDirectoryResult result = Pusher.Push(sourceDirectory, targetDirectory, "*.txt");

        //    // Check the results
        //    Assert.IsNotNull(result);
        //    Assert.IsNotNull(result.CreatedFiles);
        //    Assert.AreEqual(1, result.CreatedFiles.Count);
        //    Assert.AreEqual(soughtFile.Name, result.CreatedFiles[0]);
        //    Assert.IsNotNull(result.FailedEntries);
        //    Assert.AreEqual(0, result.FailedEntries.Count);
        //    Assert.IsNotNull(result.UpdatedFiles);
        //    Assert.AreEqual(0, result.UpdatedFiles.Count);
        //    Assert.IsNotNull(result.VerifiedFiles);
        //    Assert.AreEqual(0, result.VerifiedFiles.Count);
        //    Assert.IsNotNull(result.DirectoryResults);
        //    Assert.AreEqual(0, result.DirectoryResults.Count);
        //}
        
        //[TestMethod]
        //public void TestPushDirectoryByAttributes()
        //{
        //    // Prepare the source and target directories and files.
        //    IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
        //    IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
        //    IFile soughtFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 }, 
        //        FileAttributesWrapper.Normal);
        //    IFile unsoughtFile = Common.CreateFile("File2.txt", Common.SourceDirectoryPath, new byte[] { 65 }, 
        //        FileAttributesWrapper.Hidden);

        //    // Run the test.
        //    Pusher.PushDirectoryResult result = Pusher.Push(sourceDirectory, targetDirectory, null, 
        //        FileAttributesWrapper.Normal);

        //    // Check the results
        //    Assert.IsNotNull(result);
        //    Assert.IsNotNull(result.CreatedFiles);
        //    Assert.AreEqual(1, result.CreatedFiles.Count);
        //    Assert.AreEqual(soughtFile.Name, result.CreatedFiles[0]);
        //    Assert.IsNotNull(result.FailedEntries);
        //    Assert.AreEqual(0, result.FailedEntries.Count);
        //    Assert.IsNotNull(result.UpdatedFiles);
        //    Assert.AreEqual(0, result.UpdatedFiles.Count);
        //    Assert.IsNotNull(result.VerifiedFiles);
        //    Assert.AreEqual(0, result.VerifiedFiles.Count);
        //    Assert.IsNotNull(result.DirectoryResults);
        //    Assert.AreEqual(0, result.DirectoryResults.Count);
        //}

        //[TestMethod]
        //public void TestPushDirectoryByPatternAndAttributes()// More like SyncDirectory? UpdateDirectory
        //{
        //    // Prepare the source and target directories and files.
        //    IDirectory sourceDirectory = Common.CreateDirectory(Common.SourceDirectoryPath);
        //    IDirectory targetDirectory = Common.CreateDirectory(Common.TargetDirectoryPath);
        //    IFile soughtFile = Common.CreateFile("File.txt", Common.SourceDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Archive);
        //    IFile unsoughtFile1 = Common.CreateFile("File.doc", Common.SourceDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Archive);
        //    IFile unsoughtFile2 = Common.CreateFile("File2.txt", Common.SourceDirectoryPath, new byte[] { 65 }, FileAttributesWrapper.Normal);

        //    // Run the test.
        //    Pusher.PushDirectoryResult result = Pusher.Push(sourceDirectory, targetDirectory, "*.txt", FileAttributesWrapper.Archive);

        //    // Check the results
        //    Assert.IsNotNull(result);
        //    Assert.IsNotNull(result.CreatedFiles);
        //    Assert.AreEqual(1, result.CreatedFiles.Count);
        //    Assert.AreEqual(soughtFile.Name, result.CreatedFiles[0]);
        //    Assert.IsNotNull(result.FailedEntries);
        //    Assert.AreEqual(0, result.FailedEntries.Count);
        //    Assert.IsNotNull(result.UpdatedFiles);
        //    Assert.AreEqual(0, result.UpdatedFiles.Count);
        //    Assert.IsNotNull(result.VerifiedFiles);
        //    Assert.AreEqual(0, result.VerifiedFiles.Count);
        //    Assert.IsNotNull(result.DirectoryResults);
        //    Assert.AreEqual(0, result.DirectoryResults.Count);
        //}
        
    }
}
