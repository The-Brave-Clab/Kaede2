cmake_minimum_required(VERSION 3.27)
project(MacSaveFileDialog)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_OSX_ARCHITECTURES "arm64;x86_64")

add_library(MacSaveFileDialog SHARED source.mm)

target_link_libraries(MacSaveFileDialog "-framework AppKit")
