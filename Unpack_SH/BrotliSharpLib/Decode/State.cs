using System;
using size_t = BrotliSharpLib.Brotli.SizeT;
using BrotliDecoderState = BrotliSharpLib.Brotli.BrotliDecoderStateStruct;

namespace BrotliSharpLib {
    public static partial class Brotli {
        internal static unsafe void BrotliDecoderStateInit(ref BrotliDecoderState s) {
            BrotliDecoderStateInitWithCustomAllocators(ref s, null, null, null);
        }

        private static unsafe void BrotliDecoderStateInitWithCustomAllocators(
            ref BrotliDecoderState s, brotli_alloc_func alloc_func, brotli_free_func free_func,
            void* opaque) {
            if (alloc_func == null) {
                s.alloc_func = DefaultAllocFunc;
                s.free_func = DefaultFreeFunc;
                s.memory_manager_opaque = null;
            }
            else {
                s.alloc_func = alloc_func;
                s.free_func = free_func;
                s.memory_manager_opaque = opaque;
            }

            fixed (BrotliBitReader* br = &s.br)
                BrotliInitBitReader(br);

            s.state = BrotliRunningState.BROTLI_STATE_UNINITED;
            s.substate_metablock_header = BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NONE;
            s.substate_tree_group = BrotliRunningTreeGroupState.BROTLI_STATE_TREE_GROUP_NONE;
            s.substate_context_map = BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_NONE;
            s.substate_uncompressed = BrotliRunningUncompressedState.BROTLI_STATE_UNCOMPRESSED_NONE;
            s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_NONE;
            s.substate_decode_uint8 = BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_NONE;
            s.substate_read_block_length = BrotliRunningReadBlockLengthState.BROTLI_STATE_READ_BLOCK_LENGTH_NONE;

            s.buffer_length = 0;
            s.loop_counter = 0;
            s.pos = 0;
            s.rb_roundtrips = 0;
            s.partial_pos_out = 0;

            s.block_type_trees = null;
            s.block_len_trees = null;
            s.ringbuffer = null;
            s.ringbuffer_size = 0;
            s.new_ringbuffer_size = 0;
            s.ringbuffer_mask = 0;

            s.context_map = null;
            s.context_modes = null;
            s.dist_context_map = null;
            s.context_map_slice = null;
            s.dist_context_map_slice = null;

            s.sub_loop_counter = 0;

            s.literal_hgroup.codes = null;
            s.literal_hgroup.htrees = null;
            s.insert_copy_hgroup.codes = null;
            s.insert_copy_hgroup.htrees = null;
            s.distance_hgroup.codes = null;
            s.distance_hgroup.htrees = null;

            s.custom_dict = null;
            s.custom_dict_size = 0;

            s.is_last_metablock = false;
            s.should_wrap_ringbuffer = false;
            s.window_bits = 0;
            s.max_distance = 0;
            fixed (int* rb = s.dist_rb) {
                rb[0] = 16;
                rb[1] = 15;
                rb[2] = 11;
                rb[3] = 4;
            }
            s.dist_rb_idx = 0;
            s.block_type_trees = null;
            s.block_len_trees = null;

            s.mtf_upper_bound = 63;
        }

        internal static unsafe void BrotliDecoderStateCleanup(ref BrotliDecoderState s) {
            BrotliDecoderStateCleanupAfterMetablock(ref s);

            s.free_func(s.memory_manager_opaque, s.ringbuffer);
            s.ringbuffer = null;
            s.free_func(s.memory_manager_opaque, s.block_type_trees);
            s.block_type_trees = null;
        }

        private static unsafe void BrotliDecoderStateMetablockBegin(ref BrotliDecoderState s) {
            s.meta_block_remaining_len = 0;
            fixed (uint* bl = s.block_length) {
                bl[0] = 1U << 28;
                bl[1] = 1U << 28;
                bl[2] = 1U << 28;
            }
            fixed (uint* nbt = s.num_block_types) {
                nbt[0] = 1;
                nbt[1] = 1;
                nbt[2] = 1;
            }
            fixed (uint* btr = s.block_type_rb) {
                btr[0] = 1;
                btr[1] = 0;
                btr[2] = 1;
                btr[3] = 0;
                btr[4] = 1;
                btr[5] = 0;
            }
            s.context_map = null;
            s.context_modes = null;
            s.dist_context_map = null;
            s.context_map_slice = null;
            s.literal_htree = null;
            s.dist_context_map_slice = null;
            s.dist_htree_index = 0;
            s.context_lookup1 = null;
            s.context_lookup2 = null;
            s.literal_hgroup.codes = null;
            s.literal_hgroup.htrees = null;
            s.insert_copy_hgroup.codes = null;
            s.insert_copy_hgroup.htrees = null;
            s.distance_hgroup.codes = null;
            s.distance_hgroup.htrees = null;
        }

        private static unsafe void BrotliDecoderStateCleanupAfterMetablock(ref BrotliDecoderState s) {
            s.free_func(s.memory_manager_opaque, s.context_modes);
            s.context_modes = null;
            s.free_func(s.memory_manager_opaque, s.context_map);
            s.context_map = null;
            s.free_func(s.memory_manager_opaque, s.dist_context_map);
            s.dist_context_map = null;

            fixed (HuffmanTreeGroup* lh = &s.literal_hgroup)
            fixed (HuffmanTreeGroup* ich = &s.insert_copy_hgroup)
            fixed (HuffmanTreeGroup* dh = &s.distance_hgroup) {
                BrotliDecoderHuffmanTreeGroupRelease(ref s, lh);
                BrotliDecoderHuffmanTreeGroupRelease(ref s, ich);
                BrotliDecoderHuffmanTreeGroupRelease(ref s, dh);
            }
        }

        private static unsafe bool BrotliDecoderHuffmanTreeGroupInit(ref BrotliDecoderState s,
            HuffmanTreeGroup* group, uint alphabet_size, uint ntrees) {
            /* Pack two allocations into one */
            size_t max_table_size = (int) kMaxHuffmanTableSize[(alphabet_size + 31) >> 5];
            size_t code_size = sizeof(HuffmanCode) * ntrees * max_table_size;
            size_t htree_size = IntPtr.Size * ntrees;
            /* Pointer alignment is, hopefully, wider than sizeof(HuffmanCode). */
            var p = (HuffmanCode**) s.alloc_func(s.memory_manager_opaque, code_size + htree_size);
            group->alphabet_size = (ushort) alphabet_size;
            group->num_htrees = (ushort) ntrees;
            group->htrees = (HuffmanCode**) p;
            group->codes = (HuffmanCode*) (&p[ntrees]);
            return p != null;
        }
    }
}