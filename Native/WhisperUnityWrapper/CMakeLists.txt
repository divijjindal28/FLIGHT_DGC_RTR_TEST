cmake_minimum_required(VERSION 3.15)

project(WhisperUnityWrapper LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(WHISPER_ROOT
    "${CMAKE_CURRENT_SOURCE_DIR}/../../whisper.cpp"
)

add_subdirectory(
    "${WHISPER_ROOT}"
    "${CMAKE_CURRENT_BINARY_DIR}/whisper-build"
    EXCLUDE_FROM_ALL
)

add_library(
    WhisperUnityWrapper
    SHARED
    WhisperUnityWrapper.cpp
)

target_include_directories(
    WhisperUnityWrapper
    PRIVATE
    "${WHISPER_ROOT}/include"
)

target_link_libraries(
    WhisperUnityWrapper
    PRIVATE
    whisper
)

target_compile_definitions(
    WhisperUnityWrapper
    PRIVATE
    WHISPER_SHARED
)

set_target_properties(
    WhisperUnityWrapper
    PROPERTIES
    WINDOWS_EXPORT_ALL_SYMBOLS ON
)