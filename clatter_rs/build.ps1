cargo run --features headers --bin generate-cs
cargo clean --target-dir target/debug
cargo build --release; cp target/release/clatter_rs.dll ../Clatter/Clatter.Core/clatter_rs.dll