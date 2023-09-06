use safer_ffi::Vec as SafeVec;
use safer_ffi::ffi_export;
use fftconvolve::{fftconvolve, Mode};
use ndarray::ArrayView1;
#[cfg(feature = "headers")]
use safer_ffi::headers::Language::CSharp;

/// No-op to let the C# library check if it can load this library.
#[ffi_export]
pub fn is_ok() {
}

#[ffi_export]
pub fn ffi_convolve(input: &SafeVec<f64>, kernel: &SafeVec<f64>, output: &mut SafeVec<f64>, length: usize) {
    let input = ArrayView1::from_shape([input.len()], input).unwrap();
    let kernel = ArrayView1::from_shape([kernel.len()], kernel).unwrap();
    let convolved = fftconvolve(&input, &kernel, Mode::Same).unwrap();
    output[0..length].copy_from_slice(&convolved.as_slice().unwrap()[0..length]);
}

#[cfg(feature = "headers")]
pub fn generate_cs() -> ::std::io::Result<()> {
    let builder = safer_ffi::headers::builder().with_language(CSharp);
    if ::std::env::var("HEADERS_TO_STDOUT")
        .ok()
        .map_or(false, |it| it == "1")
    {
        builder.to_writer(::std::io::stdout()).generate()?
    } else {
        builder
            .to_file(&format!(
                "../Clatter/Clatter.Core/NativeBindings.cs"
            ))?
            .generate()?
    }
    Ok(())
}