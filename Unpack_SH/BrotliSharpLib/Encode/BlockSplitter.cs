using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct BlockSplit
        {
            public size_t num_types;  /* Amount of distinct types */
            public size_t num_blocks;  /* Amount of values in types and length */
            public byte* types;
            public uint* lengths;

            public size_t types_alloc_size;
            public size_t lengths_alloc_size;
        }

        private static unsafe size_t CountLiterals(Command* cmds, size_t num_commands)
        {
            /* Count how many we have. */
            size_t total_length = 0;
            size_t i;
            for (i = 0; i < num_commands; ++i)
            {
                total_length += cmds[i].insert_len_;
            }
            return total_length;
        }

        private static unsafe void CopyLiteralsToByteArray(Command* cmds,
            size_t num_commands,
            byte* data,
            size_t offset,
            size_t mask,
            byte* literals)
        {
            size_t pos = 0;
            size_t from_pos = offset & mask;
            size_t i;
            for (i = 0; i < num_commands; ++i)
            {
                size_t insert_len = cmds[i].insert_len_;
                if (from_pos + insert_len > mask)
                {
                    size_t head_size = mask + 1 - from_pos;
                    memcpy(literals + pos, data + from_pos, head_size);
                    from_pos = 0;
                    pos += head_size;
                    insert_len -= head_size;
                }
                if (insert_len > 0)
                {
                    memcpy(literals + pos, data + from_pos, insert_len);
                    pos += insert_len;
                }
                from_pos = (from_pos + insert_len + CommandCopyLen(&cmds[i])) & mask;
            }
        }

        private static unsafe void BrotliInitBlockSplit(BlockSplit* self)
        {
            self->num_types = 0;
            self->num_blocks = 0;
            self->types = null;
            self->lengths = null;
            self->types_alloc_size = 0;
            self->lengths_alloc_size = 0;
        }

        private static unsafe uint MyRand(uint* seed)
        {
            *seed *= 16807U;
            if (*seed == 0)
            {
                *seed = 1;
            }
            return *seed;
        }

        private static unsafe void BrotliDestroyBlockSplit(ref MemoryManager m, BlockSplit* self)
        {
            BrotliFree(ref m, self->types);
            BrotliFree(ref m, self->lengths);
        }

        private static double BitCost(size_t count)
        {
            return count == 0 ? -2.0 : FastLog2(count);
        }

        private static unsafe void BrotliSplitBlock(ref MemoryManager m,
            Command* cmds,
            size_t num_commands,
            byte* data,
            size_t pos,
            size_t mask,
            BrotliEncoderParams* params_,
            BlockSplit* literal_split,
            BlockSplit* insert_and_copy_split,
            BlockSplit* dist_split)
        {
            {
                size_t literals_count = CountLiterals(cmds, num_commands);
                byte* literals = (byte*)BrotliAllocate(ref m, literals_count * sizeof(byte));

                /* Create a continuous array of literals. */
                CopyLiteralsToByteArray(cmds, num_commands, data, pos, mask, literals);
                /* Create the block split on the array of literals.
                   Literal histograms have alphabet size 256. */
                BlockSplitterLiteral.SplitByteVector(
                    ref m, literals, literals_count,
                    kSymbolsPerLiteralHistogram, kMaxLiteralHistograms,
                    kLiteralStrideLength, kLiteralBlockSwitchCost, params_,
                    literal_split);

                BrotliFree(ref m, literals);
            }

            {
                /* Compute prefix codes for commands. */
                ushort* insert_and_copy_codes = (ushort*)BrotliAllocate(ref m, num_commands * sizeof(ushort));
                size_t i;

                for (i = 0; i < num_commands; ++i)
                {
                    insert_and_copy_codes[i] = cmds[i].cmd_prefix_;
                }
                /* Create the block split on the array of command prefixes. */
                BlockSplitterCommand.SplitByteVector(
                    ref m, insert_and_copy_codes, num_commands,
                    kSymbolsPerCommandHistogram, kMaxCommandHistograms,
                    kCommandStrideLength, kCommandBlockSwitchCost, params_,
                    insert_and_copy_split);

                /* TODO: reuse for distances? */
                BrotliFree(ref m, insert_and_copy_codes);
            }

            {
                /* Create a continuous array of distance prefixes. */
                ushort* distance_prefixes = (ushort*)BrotliAllocate(ref m, num_commands * sizeof(ushort));
                size_t j = 0;
                size_t i;

                for (i = 0; i < num_commands; ++i)
                {
                    Command* cmd = &cmds[i];
                    if (CommandCopyLen(cmd) != 0 && cmd->cmd_prefix_ >= 128)
                    {
                        distance_prefixes[j++] = cmd->dist_prefix_;
                    }
                }
                /* Create the block split on the array of distance prefixes. */
                BlockSplitterDistance.SplitByteVector(
                    ref m, distance_prefixes, j,
                    kSymbolsPerDistanceHistogram, kMaxCommandHistograms,
                    kCommandStrideLength, kDistanceBlockSwitchCost, params_,
                    dist_split);

                BrotliFree(ref m, distance_prefixes);
            }
        }
    }
}