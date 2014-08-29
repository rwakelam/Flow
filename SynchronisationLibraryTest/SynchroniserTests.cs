using System;
using WrapperLibrary;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronisationLibrary;

namespace SynchronisationTest
{
    [TestClass]
    public class SynchroniserTests
    {
        [TestMethod]
        public void TestSourceDirectoryNotFound()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory");
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Exception);
            Assert.AreEqual(typeof(SyncDirectoryNotFoundException), result.Exception.GetType());
        }

        [TestMethod]
        public void TestFileCreate()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file1 = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow);
            sourceDirectory.Files.Add("File1.txt", file1);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory");
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Created, result.FileSyncResults["File1.txt"].Result);
        }
        
        [TestMethod]
        public void TestFileUpdate()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile sourceFile = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile targetFile = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow.Subtract(new TimeSpan(0,0,5)));
            sourceDirectory.Files.Add("File1.txt", sourceFile);
            targetDirectory.Files.Add("File1.txt", targetFile);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory");
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Updated, result.FileSyncResults["File1.txt"].Result);
        }

        [TestMethod]
        public void TestUnchangedFileIgnore()
        {
            DateTime lastWriteTimeUtc = DateTime.UtcNow;
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, lastWriteTimeUtc);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, lastWriteTimeUtc);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, lastWriteTimeUtc);
            MockFile sourceFile = new MockFile(FileAttributesWrapper.Normal, lastWriteTimeUtc);
            MockFile targetFile = new MockFile(FileAttributesWrapper.Normal, lastWriteTimeUtc);
            sourceDirectory.Files.Add("File1.txt", sourceFile);
            targetDirectory.Files.Add("File1.txt", targetFile);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory");
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Unchanged, result.FileSyncResults["File1.txt"].Result);
        }

        [TestMethod]
        public void TestFileDelete()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file1 = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow);
            targetDirectory.Files.Add("File1.txt", file1);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory", null, true);
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Deleted, result.FileSyncResults["File1.txt"].Result);
        }

        [TestMethod]
        public void TestSystemFileIgnore()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file1 = new MockFile(FileAttributesWrapper.System, DateTime.UtcNow);
            sourceDirectory.Files.Add("File1.txt", file1);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory", null, true);
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Ignored, result.FileSyncResults["File1.txt"].Result);
        }

        [TestMethod]
        public void TestHiddenFileIgnore()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file1 = new MockFile(FileAttributesWrapper.Hidden, DateTime.UtcNow);
            sourceDirectory.Files.Add("File1.txt", file1);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory", null, true);
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Ignored, result.FileSyncResults["File1.txt"].Result);
        }
        
        [TestMethod]
        public void TestDirectoryCreate()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory");
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey(""));
            Assert.AreEqual(SyncResult.Created, result.FileSyncResults[""].Result);
        }

        [TestMethod]
        public void TestDirectoryDelete()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetSubDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            targetDirectory.Directories.Add("TargetSubDirectory", targetSubDirectory);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory", null, true);
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("TargetSubDirectory"));
            Assert.AreEqual(SyncResult.Deleted, result.FileSyncResults["TargetSubDirectory"].Result);
        }

        [TestMethod]
        public void TestFilePattern()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file1 = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file2 = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow);
            sourceDirectory.Files.Add("File1.txt", file1);
            sourceDirectory.Files.Add("File2.doc", file2);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory", "^.*\\.txt$");
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(1, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Created, result.FileSyncResults["File1.txt"].Result);
        }

        [TestMethod]
        public void TestFileCreateMultiple()
        {
            MockDirectory rootDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory sourceDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockDirectory targetDirectory = new MockDirectory(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file1 = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow);
            MockFile file2 = new MockFile(FileAttributesWrapper.Normal, DateTime.UtcNow);
            sourceDirectory.Files.Add("File1.txt", file1);
            sourceDirectory.Files.Add("File2.doc", file2);
            rootDirectory.Directories.Add("SourceDirectory", sourceDirectory);
            rootDirectory.Directories.Add("TargetDirectory", targetDirectory);
            Synchroniser synchroniser = new Synchroniser(new MockFileSystem(rootDirectory), @"SourceDirectory", @"TargetDirectory", "^.*\\..*$");
            DirectorySyncResult result = synchroniser.Push();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.FileSyncResults);
            Assert.AreEqual(2, result.FileSyncResults.Count);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File1.txt"));
            Assert.AreEqual(SyncResult.Created, result.FileSyncResults["File1.txt"].Result);
            Assert.IsTrue(result.FileSyncResults.ContainsKey("File2.doc"));
            Assert.AreEqual(SyncResult.Created, result.FileSyncResults["File2.doc"].Result);
        }

    }
}
