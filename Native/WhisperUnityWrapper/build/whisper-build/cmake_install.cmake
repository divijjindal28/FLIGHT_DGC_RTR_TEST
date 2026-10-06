# Install script for directory: C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/whisper.cpp

# Set the install prefix
if(NOT DEFINED CMAKE_INSTALL_PREFIX)
  set(CMAKE_INSTALL_PREFIX "C:/Program Files (x86)/WhisperUnityWrapper")
endif()
string(REGEX REPLACE "/$" "" CMAKE_INSTALL_PREFIX "${CMAKE_INSTALL_PREFIX}")

# Set the install configuration name.
if(NOT DEFINED CMAKE_INSTALL_CONFIG_NAME)
  if(BUILD_TYPE)
    string(REGEX REPLACE "^[^A-Za-z0-9_]+" ""
           CMAKE_INSTALL_CONFIG_NAME "${BUILD_TYPE}")
  else()
    set(CMAKE_INSTALL_CONFIG_NAME "Release")
  endif()
  message(STATUS "Install configuration: \"${CMAKE_INSTALL_CONFIG_NAME}\"")
endif()

# Set the component getting installed.
if(NOT CMAKE_INSTALL_COMPONENT)
  if(COMPONENT)
    message(STATUS "Install component: \"${COMPONENT}\"")
    set(CMAKE_INSTALL_COMPONENT "${COMPONENT}")
  else()
    set(CMAKE_INSTALL_COMPONENT)
  endif()
endif()

# Is this installation the result of a crosscompile?
if(NOT DEFINED CMAKE_CROSSCOMPILING)
  set(CMAKE_CROSSCOMPILING "FALSE")
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  if(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/Debug/whisper.lib")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/Release/whisper.lib")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/MinSizeRel/whisper.lib")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/RelWithDebInfo/whisper.lib")
  endif()
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  if(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/Debug/whisper.dll")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/Release/whisper.dll")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/MinSizeRel/whisper.dll")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/RelWithDebInfo/whisper.dll")
  endif()
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/include" TYPE FILE FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/whisper.cpp/include/whisper.h")
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  if(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/Debug/parakeet.lib")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/Release/parakeet.lib")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/MinSizeRel/parakeet.lib")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/RelWithDebInfo/parakeet.lib")
  endif()
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  if(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/Debug/parakeet.dll")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/Release/parakeet.dll")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/MinSizeRel/parakeet.dll")
  elseif(CMAKE_INSTALL_CONFIG_NAME MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/bin" TYPE SHARED_LIBRARY FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/bin/RelWithDebInfo/parakeet.dll")
  endif()
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/include" TYPE FILE FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/whisper.cpp/include/parakeet.h")
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/whisper" TYPE FILE FILES
    "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/whisper-config.cmake"
    "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/whisper-config-version.cmake"
    )
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/pkgconfig" TYPE FILE FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/whisper.pc")
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/parakeet" TYPE FILE FILES
    "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/parakeet-config.cmake"
    "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/parakeet-config-version.cmake"
    )
endif()

if(CMAKE_INSTALL_COMPONENT STREQUAL "Unspecified" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/pkgconfig" TYPE FILE FILES "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/parakeet.pc")
endif()

if(NOT CMAKE_INSTALL_LOCAL_ONLY)
  # Include the install script for each subdirectory.
  include("C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/ggml/cmake_install.cmake")
  include("C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/src/cmake_install.cmake")

endif()

string(REPLACE ";" "\n" CMAKE_INSTALL_MANIFEST_CONTENT
       "${CMAKE_INSTALL_MANIFEST_FILES}")
if(CMAKE_INSTALL_LOCAL_ONLY)
  file(WRITE "C:/Users/divij/Documents/UnityProjects/Projects/FLIGHT_DGC_RTR_TEST/Native/WhisperUnityWrapper/build/whisper-build/install_local_manifest.txt"
     "${CMAKE_INSTALL_MANIFEST_CONTENT}")
endif()
