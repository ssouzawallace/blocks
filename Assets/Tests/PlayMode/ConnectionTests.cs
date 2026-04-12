using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Tests for the Block.Connection inner class.
/// Covers construction, socket/connection type accessors, attach/detach logic,
/// and the TryAttachWithBlock matching algorithm.
/// </summary>
[TestFixture]
public class ConnectionTests
{
    private SimpleInstructionBlock blockA;
    private SimpleInstructionBlock blockB;

    [SetUp]
    public void SetUp()
    {
        blockA = BlockTestHelper.CreateSimpleInstructionBlock("fd 100");
        BlockTestHelper.InitializeBlock(blockA);

        blockB = BlockTestHelper.CreateSimpleInstructionBlock("rt 90");
        BlockTestHelper.InitializeBlock(blockB);
    }

    [TearDown]
    public void TearDown()
    {
        BlockTestHelper.DestroyBlock(blockA);
        BlockTestHelper.DestroyBlock(blockB);
    }

    [Test]
    public void Connection_Constructor_SetsSocketType()
    {
        var connection = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        Assert.AreEqual(Block.Connection.SocketType.SocketTypeMale, connection.GetSocketType());
    }

    [Test]
    public void Connection_Constructor_InitializesAttachedBlockAsNull()
    {
        var connection = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        Assert.IsNull(connection.GetAttachedBlock());
    }

    [Test]
    public void Connection_SetRelativePosition_UpdatesPosition()
    {
        var connection = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(10, 20));

        Vector2 newPos = new Vector2(30, 40);
        connection.SetRelativePosition(newPos);

        Assert.AreEqual(newPos, connection.GetRelativePosition());
    }

    [Test]
    public void Connection_GetRelativePosition_ReturnsInitialPosition()
    {
        Vector2 initialPos = new Vector2(15, 25);
        var connection = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            initialPos);

        Assert.AreEqual(initialPos, connection.GetRelativePosition());
    }

    [Test]
    public void Connection_Attach_SetsAttachedBlock()
    {
        var connA = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        var connB = new Block.Connection(
            blockB,
            Block.Connection.SocketType.SocketTypeFemale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        connA.Attach(blockB, connB);

        Assert.AreEqual(blockB, connA.GetAttachedBlock());
    }

    [Test]
    public void Connection_Attach_SetsReciprocal()
    {
        var connA = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        var connB = new Block.Connection(
            blockB,
            Block.Connection.SocketType.SocketTypeFemale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        connA.Attach(blockB, connB);

        // Reciprocal: connB should now point to blockA
        Assert.AreEqual(blockA, connB.GetAttachedBlock());
    }

    [Test]
    public void Connection_Attach_DoesNotOverwriteExisting()
    {
        var connA = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        var connB = new Block.Connection(
            blockB,
            Block.Connection.SocketType.SocketTypeFemale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        // Create a third block
        var blockC = BlockTestHelper.CreateSimpleInstructionBlock("lt 45");
        BlockTestHelper.InitializeBlock(blockC);

        var connC = new Block.Connection(
            blockC,
            Block.Connection.SocketType.SocketTypeFemale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        // First attach
        connA.Attach(blockB, connB);
        // Second attach should not overwrite
        connA.Attach(blockC, connC);

        Assert.AreEqual(blockB, connA.GetAttachedBlock());

        BlockTestHelper.DestroyBlock(blockC);
    }

    [Test]
    public void Connection_Detach_ClearsAttachedBlock()
    {
        // Use the block's own connections (set up by Start()) for proper detach test
        // blockA.connections[1] is the Male/Next connection
        // blockB.connections[0] is the Female/Top connection
        var connMale = blockA.connections[1] as Block.Connection;
        var connFemale = blockB.connections[0] as Block.Connection;

        connMale.Attach(blockB, connFemale);
        Assert.AreEqual(blockB, connMale.GetAttachedBlock());

        connMale.Detach();
        Assert.IsNull(connMale.GetAttachedBlock());
    }

    [Test]
    public void Connection_Detach_ClearsReciprocal()
    {
        var connMale = blockA.connections[1] as Block.Connection;
        var connFemale = blockB.connections[0] as Block.Connection;

        connMale.Attach(blockB, connFemale);
        connMale.Detach();

        Assert.IsNull(connFemale.GetAttachedBlock());
    }

    [Test]
    public void Connection_Detach_WhenNotAttached_DoesNothing()
    {
        var conn = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0));

        // Should not throw
        Assert.DoesNotThrow(() => conn.Detach());
        Assert.IsNull(conn.GetAttachedBlock());
    }

    [Test]
    public void Connection_DistanceTo_ReturnsCorrectDistance()
    {
        // Place blocks at known positions
        blockA.transform.position = new Vector3(0, 0, 0);
        blockB.transform.position = new Vector3(100, 0, 0);

        // Both connections with absolute offset (xOffsetType=true, yOffsetType=true)
        var connA = new Block.Connection(
            blockA,
            Block.Connection.SocketType.SocketTypeMale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0),
            true, true);

        var connB = new Block.Connection(
            blockB,
            Block.Connection.SocketType.SocketTypeFemale,
            Block.Connection.ConnectionType.ConnectionTypeRegular,
            new Vector2(0, 0),
            true, true);

        float distance = connA.DistanceTo(connB);
        Assert.AreEqual(100f, distance, 0.01f);
    }

    [Test]
    public void Connection_TryAttachWithBlock_MatchingConnections_ReturnsTrue()
    {
        // Place blocks very close together so they are within attach radius
        blockA.transform.position = new Vector3(0, 0, 0);
        blockB.transform.position = new Vector3(0, 0, 0);

        // blockA.connections[1] is Male/Regular (next)
        // blockB.connections[0] is Female/Regular (top)
        // They have opposite socket types and same connection type
        var connMale = blockA.connections[1] as Block.Connection;

        bool result = connMale.TryAttachWithBlock(blockB);

        Assert.IsTrue(result);
    }

    [Test]
    public void Connection_TryAttachWithBlock_SameSocketType_ReturnsFalse()
    {
        blockA.transform.position = new Vector3(0, 0, 0);
        blockB.transform.position = new Vector3(0, 0, 0);

        // blockA.connections[0] is Female/Regular
        // blockB.connections[0] is also Female/Regular
        // Same socket type should not match
        var connFemale = blockA.connections[0] as Block.Connection;

        bool result = connFemale.TryAttachWithBlock(blockB);

        Assert.IsFalse(result);
    }

    [Test]
    public void Connection_TryAttachWithBlock_DifferentConnectionType_ReturnsFalse()
    {
        blockA.transform.position = new Vector3(0, 0, 0);

        // Create a block with Number type connections
        var numberBlock = BlockTestHelper.CreateConstantNumberBlock(5);
        BlockTestHelper.InitializeBlock(numberBlock);
        numberBlock.transform.position = new Vector3(0, 0, 0);

        // blockA has Regular connections, numberBlock has Number connections
        // They should not match due to different ConnectionType
        var connMale = blockA.connections[1] as Block.Connection;
        bool result = connMale.TryAttachWithBlock(numberBlock);

        Assert.IsFalse(result);

        BlockTestHelper.DestroyBlock(numberBlock);
    }

    [Test]
    public void Connection_TryAttachWithBlock_TooFarApart_ReturnsFalse()
    {
        // Place blocks far apart (beyond kMinimumAttachRadius of 20)
        blockA.transform.position = new Vector3(0, 0, 0);
        blockB.transform.position = new Vector3(1000, 1000, 0);

        var connMale = blockA.connections[1] as Block.Connection;

        bool result = connMale.TryAttachWithBlock(blockB);

        Assert.IsFalse(result);
    }

    [Test]
    public void Connection_TryAttachWithBlock_AlreadyAttached_ReturnsFalse()
    {
        blockA.transform.position = new Vector3(0, 0, 0);
        blockB.transform.position = new Vector3(0, 0, 0);

        var connMale = blockA.connections[1] as Block.Connection;

        // First attach should succeed
        bool result1 = connMale.TryAttachWithBlock(blockB);
        Assert.IsTrue(result1);

        // Create another block and try to attach to same connection
        var blockC = BlockTestHelper.CreateSimpleInstructionBlock("lt 45");
        BlockTestHelper.InitializeBlock(blockC);
        blockC.transform.position = new Vector3(0, 0, 0);

        bool result2 = connMale.TryAttachWithBlock(blockC);
        Assert.IsFalse(result2);

        BlockTestHelper.DestroyBlock(blockC);
    }
}
