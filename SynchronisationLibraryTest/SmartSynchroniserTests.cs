using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronisationLibrary;
using System.IO;
using System.Threading;
using WrapperLibrary;

namespace SynchronisationTest
{
    /// <summary>
    /// Summary description for SmartSynchroniserTests
    /// </summary>
    [TestClass]
    public class SmartSynchroniserTests
    {
        private const string SourceDirectory = @"C:\Temp\Source";
        private const string TargetDirectory = @"C:\Temp\Target";
        private IFileSystemWrapper _fileSystem;

        public SmartSynchroniserTests()
        {
            _fileSystem = FileSystemWrapper.GetFileSystemWrapper();
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestCreateFile()
        {
            // Prepare the source/target directories.
            string sourcePath = _fileSystem.BuildPath(SourceDirectory, "Test.txt");
            string targetPath = _fileSystem.BuildPath(TargetDirectory, "Test.txt");
            if (_fileSystem.FileExists(sourcePath))
            {
                _fileSystem.DeleteFile(sourcePath);
            }
            if (_fileSystem.FileExists(targetPath))
            {
                _fileSystem.DeleteFile(targetPath);
            }

            // Run the test.
            SmartSynchroniser synchroniser =
                new SmartSynchroniser(_fileSystem, SourceDirectory, TargetDirectory, true, "*.*");
            synchroniser.Enabled = true;

            File.WriteAllText(sourcePath, "TestCreateFile");

            Thread.Sleep(1000);
            synchroniser.Synchronise();
            Assert.IsTrue(_fileSystem.FileExists(targetPath));
            Assert.AreEqual("TestCreateFile", File.ReadAllText(targetPath));
        }

        [TestMethod]
        public void TestUpdateFile()
        {
            // Prepare the source/target directories.
            string sourcePath = _fileSystem.BuildPath(SourceDirectory, "Test.txt");
            string targetPath = _fileSystem.BuildPath(TargetDirectory, "Test.txt");
            if (_fileSystem.FileExists(sourcePath))
            {
                _fileSystem.DeleteFile(sourcePath);
            }
            if (_fileSystem.FileExists(targetPath))
            {
                _fileSystem.DeleteFile(targetPath);
            }
            File.WriteAllText(sourcePath, "TestCreateFile");
            File.WriteAllText(targetPath, "TestCreateFile");

            // Run the test.
            SmartSynchroniser synchroniser =
                new SmartSynchroniser(_fileSystem, SourceDirectory, TargetDirectory, true, "*.*");
            synchroniser.Enabled = true;


            File.WriteAllText(sourcePath, "TestUpdateFile");

            Thread.Sleep(1000);
            synchroniser.Synchronise();
            Assert.IsTrue(_fileSystem.FileExists(targetPath));
            Assert.AreEqual("TestUpdateFile", File.ReadAllText(targetPath));
        }

        [TestMethod]
        public void TestDeleteFile()
        {
            // Prepare the source/target directories.
            string sourcePath = _fileSystem.BuildPath(SourceDirectory, "Test.txt");
            string targetPath = _fileSystem.BuildPath(TargetDirectory, "Test.txt");
            if (!_fileSystem.FileExists(sourcePath))
            {
                File.Create(sourcePath);
            }
            if (!_fileSystem.FileExists(targetPath))
            {
                File.Create(targetPath);
            }

            // Run the test.
            SmartSynchroniser synchroniser =
                new SmartSynchroniser(_fileSystem, SourceDirectory, TargetDirectory, true, "*.*");
            synchroniser.Enabled = true;

            _fileSystem.DeleteFile(sourcePath);

            Thread.Sleep(1000);
            synchroniser.Synchronise();
            Assert.IsFalse(_fileSystem.FileExists(targetPath));
        }


        [TestMethod]
        public void TestRenameFile()
        {
            // Prepare the source/target directories.
            string sourcePath = _fileSystem.BuildPath(SourceDirectory, "Test.txt");
            string targetPath = _fileSystem.BuildPath(TargetDirectory, "Test.txt");
            string newSourcePath = _fileSystem.BuildPath(SourceDirectory, "Test2.txt");
            string newTargetPath = _fileSystem.BuildPath(TargetDirectory, "Test2.txt");
            File.WriteAllText(sourcePath, "TestRenameFile");
            File.WriteAllText(targetPath, "TestRenameFile");
            if (_fileSystem.FileExists(newSourcePath))
            {
                _fileSystem.DeleteFile(newSourcePath);
            }
            if (_fileSystem.FileExists(newTargetPath))
            {
                _fileSystem.DeleteFile(newTargetPath);
            }

            // Run the test.
            SmartSynchroniser synchroniser =
                new SmartSynchroniser(_fileSystem, SourceDirectory, TargetDirectory, true, "*.*");
            synchroniser.Enabled = true;

            File.Move(sourcePath, newSourcePath);

            Thread.Sleep(1000);
            synchroniser.Synchronise();
            Assert.IsTrue(_fileSystem.FileExists(newTargetPath));
            Assert.IsFalse(_fileSystem.FileExists(targetPath));
            Assert.AreEqual("TestRenameFile", File.ReadAllText(newTargetPath));
        }
        
        [TestMethod]
        public void TestDeleteDirectory()
        {
            // Prepare the source/target directories.
            string sourcePath = _fileSystem.BuildPath(SourceDirectory, "SubDirectory");
            string targetPath = _fileSystem.BuildPath(TargetDirectory, "SubDirectory");
            string sourceFile = _fileSystem.BuildPath(sourcePath, "Test.txt");
            string targetFile = _fileSystem.BuildPath(targetPath, "Test.txt");
            if (!_fileSystem.DirectoryExists(sourcePath))
            {
                _fileSystem.CreateDirectory(sourcePath);
            }
            if (!_fileSystem.DirectoryExists(targetPath))
            {
                _fileSystem.CreateDirectory(targetPath);
            }
            if (!_fileSystem.FileExists(sourceFile))
            {
                File.Create(sourceFile);
            }
            Thread.Sleep(5000);

            // Run the test.
            SmartSynchroniser synchroniser =
                new SmartSynchroniser(_fileSystem, SourceDirectory, TargetDirectory, true, "*.*");
            synchroniser.Enabled = true;

            _fileSystem.DeleteDirectory(sourcePath, true);

            Thread.Sleep(30000);
            synchroniser.Synchronise();
            Assert.IsFalse(_fileSystem.FileExists(targetFile));
            Assert.IsFalse(_fileSystem.DirectoryExists(targetPath));
        }
    }
}
