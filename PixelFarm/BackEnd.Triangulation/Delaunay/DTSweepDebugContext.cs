/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace Poly2Tri
{

#if DEBUG
    public class dbugDTSweepContext : TriangulationDebugContext
    {
        /*
         * Fields used for visual representation of current triangulation
         */

        DelaunayTriangle _primaryTriangle;
        DelaunayTriangle _secondaryTriangle;
        TriangulationPoint _activePoint;
        AdvancingFrontNode _activeNode;
        DTSweepConstraint _activeConstraint;

        public dbugDTSweepContext(DTSweepContext tcx) : base(tcx) { }

        public DelaunayTriangle PrimaryTriangle
        {
            get => _primaryTriangle;
            set
            {
                _primaryTriangle = value;
                _tcx.Update("set PrimaryTriangle");
            }
        }
        public DelaunayTriangle SecondaryTriangle
        {
            get => _secondaryTriangle;
            set
            {
                _secondaryTriangle = value;
                _tcx.Update("set SecondaryTriangle");
            }
        }
        public TriangulationPoint ActivePoint
        {
            get => _activePoint;
            set
            {
                _activePoint = value;
                _tcx.Update("set ActivePoint");
            }
        }
        internal AdvancingFrontNode ActiveNode
        {
            get => _activeNode;
            set
            {
                _activeNode = value;
                _tcx.Update("set ActiveNode");
            }
        }
        public DTSweepConstraint ActiveConstraint
        {
            get => _activeConstraint;
            set
            {
                _activeConstraint = value;
                _tcx.Update("set ActiveConstraint");
            }
        }
        public bool IsDebugContext => true;

        public override void Clear()
        {
            PrimaryTriangle = null;
            SecondaryTriangle = null;
            ActivePoint = null;
            ActiveNode = null;
            ActiveConstraint = DTSweepConstraint.Empty;
        }
    }
#endif
}
