//MIT, 2019, master131, https://github.com/master131/BrotliSharpLib
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    /// <summary>
    /// Represents a Brotli stream for compression or decompression.
    /// </summary>
    public unsafe class BrotliStream : Stream {
        private Stream _stream;
        private CompressionMode _mode;
        private bool _leaveOpen, _disposed;
        private IntPtr _customDictionary = IntPtr.Zero;
        private byte[] _buffer;
        private int _bufferCount, _bufferOffset;

        private Brotli.BrotliEncoderStateStruct _encoderState;
        private Brotli.BrotliDecoderStateStruct _decoderState;

        private Brotli.BrotliDecoderResult _lastDecoderState =
            Brotli.BrotliDecoderResult.BROTLI_DECODER_RESULT_NEEDS_MORE_INPUT;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrotliStream"/> class using the specified stream and
        /// compression mode, and optionally leaves the stream open.
        /// </summary>
        /// <param name="stream">The stream to compress or decompress.</param>
        /// <param name="mode">One of the enumeration values that indicates whether to compress or decompress the stream.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after disposing the <see cref="BrotliStream"/> object; otherwise, <c>false</c>.</param>
        public BrotliStream(Stream stream, CompressionMode mode, bool leaveOpen) {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (CompressionMode.Compress != mode && CompressionMode.Decompress != mode)
                throw new ArgumentOutOfRangeException(nameof(mode));

            _stream = stream;
            _mode = mode;
            _leaveOpen = leaveOpen;

            switch (_mode) {
                case CompressionMode.Decompress:
                    if (!_stream.CanRead)
                        throw new ArgumentException("Stream does not support read", nameof(stream));

                    _decoderState = Brotli.BrotliCreateDecoderState();
                    Brotli.BrotliDecoderStateInit(ref _decoderState);
                    _buffer = new byte[0xfff0];
                    break;
                case CompressionMode.Compress:
                    if (!_stream.CanWrite)
                        throw new ArgumentException("Stream does not support write", nameof(stream));

                    _encoderState = Brotli.BrotliEncoderCreateInstance(null, null, null);
                    SetQuality(1);
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrotliStream"/> class using the specified stream and
        /// compression mode.
        /// </summary>
        /// <param name="stream">The stream to compress or decompress.</param>
        /// <param name="mode">One of the enumeration values that indicates whether to compress or decompress the stream.</param>
        public BrotliStream(Stream stream, CompressionMode mode) :
            this(stream, mode, false) {
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector reclaims the <see cref="BrotliStream"/>.
        /// </summary>
        ~BrotliStream()
        {
            Dispose(false);
        }

        /// <summary>
        /// Sets the quality for compression.
        /// </summary>
        /// <param name="quality">The quality value (a value from 0-11).</param>
        public void SetQuality(int quality) {
            if (_mode != CompressionMode.Compress)
                throw new InvalidOperationException("SetQuality is only valid for compress");

            if (quality < Brotli.BROTLI_MIN_QUALITY || quality > Brotli.BROTLI_MAX_QUALITY)
                throw new ArgumentOutOfRangeException(nameof(quality), "Quality should be a value between " +
                                                                       Brotli.BROTLI_MIN_QUALITY + "-" + Brotli
                                                                           .BROTLI_MAX_QUALITY);

            EnsureNotDisposed();

            Brotli.BrotliEncoderSetParameter(ref _encoderState, Brotli.BrotliEncoderParameter.BROTLI_PARAM_QUALITY,
                (uint) quality);
        }

        /// <summary>
        /// Sets the dictionary for compression and decompression.
        /// </summary>
        /// <param name="dictionary">The dictionary as a byte array.</param>
        public void SetCustomDictionary(byte[] dictionary) {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            EnsureNotDisposed();

            if (_customDictionary != IntPtr.Zero)
                Marshal.FreeHGlobal(_customDictionary);

            _customDictionary = Marshal.AllocHGlobal(dictionary.Length);
            Marshal.Copy(dictionary, 0, _customDictionary, dictionary.Length);

            if (_mode == CompressionMode.Compress) {
                Brotli.BrotliEncoderSetCustomDictionary(ref _encoderState, dictionary.Length,
                    (byte*) _customDictionary);
            }
            else {
                Brotli.BrotliDecoderSetCustomDictionary(ref _decoderState, dictionary.Length,
                    (byte*) _customDictionary);
            }
        }

        /// <summary>
        /// Sets the window size for the encoder
        /// </summary>
        /// <param name="windowSize">The window size in bits (a value from 10-24)</param>
        public void SetWindow(int windowSize) {
            if (_mode != CompressionMode.Compress)
                throw new InvalidOperationException("SetWindow is only valid for compress");

            if (windowSize < Brotli.BROTLI_MIN_WINDOW_BITS || windowSize > Brotli.BROTLI_MAX_WINDOW_BITS)
                throw new ArgumentOutOfRangeException(nameof(windowSize), "Window size should be a value between " +
                                                                          Brotli.BROTLI_MIN_WINDOW_BITS + "-" + Brotli
                                                                              .BROTLI_MAX_WINDOW_BITS);

            EnsureNotDisposed();

            Brotli.BrotliEncoderSetParameter(ref _encoderState, Brotli.BrotliEncoderParameter.BROTLI_PARAM_LGWIN,
                (uint) windowSize);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="BrotliStream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                FlushCompress(true);

                if (_mode == CompressionMode.Compress)
                    Brotli.BrotliEncoderDestroyInstance(ref _encoderState);
                else
                    Brotli.BrotliDecoderStateCleanup(ref _decoderState);
                if (_customDictionary != IntPtr.Zero) {
                    Marshal.FreeHGlobal(_customDictionary);
                    _customDictionary = IntPtr.Zero;
                }
                _disposed = true;
            }

            if (disposing && !_leaveOpen && _stream != null) {
                _stream.Dispose();
                _stream = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Flushes any buffered data into the stream
        /// </summary>
        public override void Flush() {
            EnsureNotDisposed();
            FlushCompress(false);
        }

        private void FlushCompress(bool finish) {
            if (_mode != CompressionMode.Compress)
                return;

            if (Brotli.BrotliEncoderIsFinished(ref _encoderState))
                return;

            var op = finish
                ? Brotli.BrotliEncoderOperation.BROTLI_OPERATION_FINISH
                : Brotli.BrotliEncoderOperation.BROTLI_OPERATION_FLUSH;

            byte[] buffer = new byte[0];
            WriteCore(buffer, 0, 0, op);
        }

        /// <summary>
        /// This operation is not supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This operation is not supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        private void ValidateParameters(byte[] array, int offset, int count) {
            if (array == null)
                throw new ArgumentNullException("array");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (array.Length - offset < count)
                throw new ArgumentException("Invalid argument offset and count");
        }

        /// <summary>
        /// Reads a number of decompressed bytes into the specified byte array.
        /// </summary>
        /// <param name="buffer">The array to store decompressed bytes.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of decompressed bytes to read.</param>
        /// <returns>The number of bytes that were read into the byte array.</returns>
        public override int Read(byte[] buffer, int offset, int count) {
            if (_mode != CompressionMode.Decompress)
                throw new InvalidOperationException("Read is only supported in Decompress mode");

            EnsureNotDisposed();
            ValidateParameters(buffer, offset, count);

            int totalWritten = 0;
            while (offset < buffer.Length && _lastDecoderState != Brotli.BrotliDecoderResult.BROTLI_DECODER_RESULT_SUCCESS) {
                if (_lastDecoderState == Brotli.BrotliDecoderResult.BROTLI_DECODER_RESULT_NEEDS_MORE_INPUT) {
                    if (_bufferCount > 0 && _bufferOffset != 0) {
                        Array.Copy(_buffer, _bufferOffset, _buffer, 0, _bufferCount);
                    }
                    _bufferOffset = 0;

                    int numRead = 0;
                    while (_bufferCount < _buffer.Length && ((numRead = _stream.Read(_buffer, _bufferCount, _buffer.Length - _bufferCount)) > 0)) {
                        _bufferCount += numRead;
                        if (_bufferCount > _buffer.Length)
                            throw new InvalidDataException("Invalid input stream detected, more bytes supplied than expected.");
                    }

                    if (_bufferCount <= 0)
                        break;
                }

                size_t available_in = _bufferCount;
                size_t available_in_old = available_in;
                size_t available_out = count;
                size_t available_out_old = available_out;

                fixed (byte* out_buf_ptr = buffer)
                fixed (byte* in_buf_ptr = _buffer) {
                    byte* in_buf = in_buf_ptr + _bufferOffset;
                    byte* out_buf = out_buf_ptr + offset;
                    _lastDecoderState = Brotli.BrotliDecoderDecompressStream(ref _decoderState, &available_in, &in_buf,
                        &available_out, &out_buf, null);
                }

                if (_lastDecoderState == Brotli.BrotliDecoderResult.BROTLI_DECODER_RESULT_ERROR)
                    throw new InvalidDataException("Decompression failed with error code: " + _decoderState.error_code);

                size_t bytesConsumed = available_in_old - available_in;
                size_t bytesWritten = available_out_old - available_out;

                if (bytesConsumed > 0) {
                    _bufferOffset += (int) bytesConsumed;
                    _bufferCount -= (int) bytesConsumed;
                }

                if (bytesWritten > 0) {
                    totalWritten += (int)bytesWritten;
                    offset += (int)bytesWritten;
                }
            }

            return totalWritten;
        }

        private void WriteCore(byte[] buffer, int offset, int count, Brotli.BrotliEncoderOperation operation) {
            bool flush = operation == Brotli.BrotliEncoderOperation.BROTLI_OPERATION_FLUSH ||
                         operation == Brotli.BrotliEncoderOperation.BROTLI_OPERATION_FINISH;

            byte[] out_buf = new byte[0x1FFFE];
            size_t available_in = count, available_out = out_buf.Length;
            fixed (byte* out_buf_ptr = out_buf)
            fixed (byte* buf_ptr = buffer) {
                byte* next_in = buf_ptr + offset;
                byte* next_out = out_buf_ptr;

                while ((!flush && available_in > 0) || flush) {
                    if (!Brotli.BrotliEncoderCompressStream(ref _encoderState,
                        operation, &available_in, &next_in,
                        &available_out, &next_out, null)) {
                        throw new InvalidDataException("Compression failed");
                    }

                    bool hasData = available_out != out_buf.Length;
                    if (hasData) {
                        int out_size = (int)(out_buf.Length - available_out);
                        _stream.Write(out_buf, 0, out_size);
                        available_out = out_buf.Length;
                        next_out = out_buf_ptr;
                    }

                    if (Brotli.BrotliEncoderIsFinished(ref _encoderState))
                        break;

                    if (!hasData && flush)
                        break;
                }
            }
        }

        /// <summary>
        /// Writes compressed bytes to the underlying stream from the specified byte array.
        /// </summary>
        /// <param name="buffer">The buffer that contains the data to compress.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> from which the bytes will be read.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count) {
            if (_mode != CompressionMode.Compress)
                throw new InvalidOperationException("Write is only supported in Compress mode");

            EnsureNotDisposed();
            ValidateParameters(buffer, offset, count);
            WriteCore(buffer, offset, count, Brotli.BrotliEncoderOperation.BROTLI_OPERATION_PROCESS);
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports reading while decompressing a file.
        /// </summary>
        public override bool CanRead {
            get {
                if (_stream == null)
                    return false;

                return _mode == CompressionMode.Decompress && _stream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// Gets a value indicating whether the stream supports writing.
        /// </summary>
        public override bool CanWrite {
            get {
                if (_stream == null)
                    return false;

                return _mode == CompressionMode.Compress && _stream.CanWrite;
            }
        }

        /// <summary>
        /// This property is not supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Length {
            get {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// This property is not supported and always throws a <see cref="NotSupportedException"/>.
        /// </summary>
        public override long Position {
            get {
                throw new NotSupportedException();
            }
            set {
                throw new NotSupportedException();
            }
        }

        private void EnsureNotDisposed() {
            if (_stream == null)
                throw new ObjectDisposedException(null, "The underlying stream has been disposed");

            if (_disposed)
                throw new ObjectDisposedException(null);
        }
    }
}
