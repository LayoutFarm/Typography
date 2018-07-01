/*
** License Applicability. Except to the extent portions of this file are
** made subject to an alternative license as permitted in the SGI Free
** Software License B, Version 1.1 (the "License"), the contents of this
** file are subject only to the provisions of the License. You may not use
** this file except in compliance with the License. You may obtain a copy
** of the License at Silicon Graphics, Inc., attn: Legal Services, 1600
** Amphitheatre Parkway, Mountain View, CA 94043-1351, or at:
** 
** http://oss.sgi.com/projects/FreeB
** 
** Note that, as provided in the License, the Software is distributed on an
** "AS IS" basis, with ALL EXPRESS AND IMPLIED WARRANTIES AND CONDITIONS
** DISCLAIMED, INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES AND
** CONDITIONS OF MERCHANTABILITY, SATISFACTORY QUALITY, FITNESS FOR A
** PARTICULAR PURPOSE, AND NON-INFRINGEMENT.
** 
** Original Code. The Original Code is: OpenGL Sample Implementation,
** Version 1.2.1, released January 26, 2000, developed by Silicon Graphics,
** Inc. The Original Code is Copyright (c) 1991-2000 Silicon Graphics, Inc.
** Copyright in any portions created by third parties is as indicated
** elsewhere herein. All Rights Reserved.
** 
** Additional Notice Provisions: The application programming interfaces
** established by SGI in conjunction with the Original Code are The
** OpenGL(R) Graphics System: A Specification (Version 1.2.1), released
** April 1, 1999; The OpenGL(R) Graphics System Utility Library (Version
** 1.3), released November 4, 1998; and OpenGL(R) Graphics with the X
** Window System(R) (Version 1.3), released October 19, 1998. This software
** was created using the OpenGL(R) version 1.2.1 Sample Implementation
** published by SGI, but has not been independently verified as being
** compliant with the OpenGL(R) version 1.2.1 Specification.
**
*/
/*
** Author: Eric Veach, July 1994.
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
*/

using DictKey = Tesselate.ActiveRegion;
namespace Tesselate
{
    public class Dictionary
    {
        public Node head = new Node();
        public Tesselator tesseator;
        public class Node
        {
            DictKey key = new DictKey();
            public Node next;
            public Node prev;
            public DictKey Key
            {
                get { return this.key; }
                set { this.key = value; }
            }

            public void Delete()
            {
                this.next.prev = this.prev;
                this.prev.next = this.next;
            }
        };
        public Dictionary(Tesselator tesselator)
        {
            Node nodeHead;
            nodeHead = this.head;
            nodeHead.Key = null;
            nodeHead.next = nodeHead;
            nodeHead.prev = nodeHead;
            this.tesseator = tesselator;
        }

        public static void dictDeleteDict(Dictionary dict)
        {
            Node node, next;
            for (node = dict.head.next; node != dict.head; node = next)
            {
                next = node.next;
                node = null;
            }

            dict = null;
        }

        public Dictionary.Node Insert(DictKey k)
        {
            return this.InsertBefore(head, k);
        }

        public Node InsertBefore(Node node, DictKey key)
        {
            Node newNode;
            do
            {
                node = node.prev;
            } while (node.Key != null &&
                !ActiveRegion.EdgeLeq(this.tesseator, node.Key, key));
            newNode = new Node();
            newNode.Key = key;
            newNode.next = node.next;
            node.next.prev = newNode;
            newNode.prev = node;
            node.next = newNode;
            return newNode;
        }

        public Dictionary.Node GetMinNode()
        {
            return this.head.next;
        }

        public static Node dictSearch(Dictionary dict, DictKey key)
        {
            Node node = dict.head;
            do
            {
                node = node.next;
            } while (node.Key != null && !ActiveRegion.EdgeLeq(dict.tesseator, key, node.Key));
            return node;
        }
    }
}