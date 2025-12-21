"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { API_BASE } from '../../lib/config';

interface UserProfile {
    sub: string;
    name: string;
    email: string;
    picture: string;
}

interface FileItem {
    id: string;
    filename: string;
    original_filename: string;
    mime_type: string;
    size: number;
    created_at: string;
    accessibleURL: string;
    views: number;
    cdn_file_name: string;
}

interface FolderFile {
    id: string;
    cdn_file_name: string;
    created_at: string;
    mime_type: string;
    original_filename: string;
    size: number;
}

interface FileListResponse {
    uploads: FileItem[];
    totalUploads: number;
    totalPages: number;
    cdnPrefix: string;
}

interface FileStats {
    uploads: number;
    storageUsed: number;
}

interface Folder {
    id: string;
    name: string;
    slug: string;
    created_at: string;
    file_count: number;
    public: boolean;
    files: FolderFile[];
}

interface FolderListResponse {
    folders: Folder[];
    totalFolders: number;
    totalPages: number;
}

type SortColumn = "created_at" | "views" | "size" | "filename";
type SortDirection = "asc" | "desc";

export default function Dashboard() {
    const router = useRouter();
    const [user, setUser] = useState<UserProfile | null>(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState("overview");

    const [stats, setStats] = useState<FileStats | null>(null);
    const [files, setFiles] = useState<FileItem[]>([]);
    const [folders, setFolders] = useState<Folder[]>([]);
    const [cdnPrefix, setCdnPrefix] = useState("https://cdn.nest.rip/uploads");
    const [filePage, setFilePage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalFiles, setTotalFiles] = useState(0);
    const [sortColumn, setSortColumn] = useState<SortColumn>("created_at");
    const [sortDirection, setSortDirection] = useState<SortDirection>("desc");
    const [renamingFolderId, setRenamingFolderId] = useState<string | null>(null);
    const [renamingFolderName, setRenamingFolderName] = useState("");

    const [selectedFiles, setSelectedFiles] = useState<Set<string>>(new Set());
    const [currentFolder, setCurrentFolder] = useState<Folder | null>(null);
    const [showMoveModal, setShowMoveModal] = useState(false);

    useEffect(() => {
        const fetchUser = async () => {
            try {
                const profileRes = await fetch(`${API_BASE}/me`, { credentials: "include" });
                if (profileRes.status === 401) { router.push("/"); return; }
                if (profileRes.ok) setUser(await profileRes.json());
                setLoading(false);
            } catch { setLoading(false); }
        };
        fetchUser();
    }, [router]);

    const fetchFiles = async () => {
        const opts = { credentials: "include" as RequestCredentials };
        const f = await fetch(`${API_BASE}/files?limit=50&page=${filePage}&sortColumn=${sortColumn}&sortDirection=${sortDirection}`, opts);
        if (f.ok) {
            const data: FileListResponse = await f.json();
            setFiles(data.uploads || []);
            setCdnPrefix(data.cdnPrefix || "https://cdn.nest.rip/uploads");
            setTotalPages(data.totalPages || 1);
            setTotalFiles(data.totalUploads || 0);
        }
    };

    const fetchFolders = async () => {
        const opts = { credentials: "include" as RequestCredentials };
        const f = await fetch(`${API_BASE}/folders`, opts);
        if (f.ok) {
            const data: FolderListResponse = await f.json();
            setFolders(data.folders || []);
            if (currentFolder) {
                const updated = data.folders?.find(folder => folder.id === currentFolder.id);
                if (updated) setCurrentFolder(updated);
            }
        }
    };

    useEffect(() => {
        if (!user) return;

        const fetchDetails = async () => {
            const opts = { credentials: "include" as RequestCredentials };

            try {
                if (activeTab === "overview") {
                    const s = await fetch(`${API_BASE}/stats`, opts);
                    if (s.ok) setStats(await s.json());
                }
                if (activeTab === "files") {
                    await fetchFiles();
                    await fetchFolders();
                }
                if (activeTab === "folders") {
                    await fetchFolders();
                }
            } catch (e) {
                console.error("Fetch error", e);
            }
        };
        fetchDetails();
    }, [activeTab, user, filePage, sortColumn, sortDirection]);

    const handleSort = (column: SortColumn) => {
        if (sortColumn === column) {
            setSortDirection(d => d === "asc" ? "desc" : "asc");
        } else {
            setSortColumn(column);
            setSortDirection("desc");
        }
        setFilePage(1);
    };

    const handleDeleteFile = async (id: string) => {
        if (!confirm("Delete file?")) return;
        await fetch(`${API_BASE}/files/${id}`, { method: "DELETE", credentials: "include" });
        setFiles(files.filter(f => f.id !== id));
        setSelectedFiles(prev => { prev.delete(id); return new Set(prev); });
        await fetchFolders();
    };

    const handleCreateFolder = async () => {
        const name = prompt("Enter folder name (min 3 characters):");
        if (!name) return;

        if (name.length < 3) {
            alert("Folder name must be at least 3 characters");
            return;
        }

        const res = await fetch(`${API_BASE}/folders`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name }),
            credentials: "include"
        });

        if (res.ok) {
            await fetchFolders();
        } else {
            try {
                const error = await res.json();
                alert(error.message || "Failed to create folder");
            } catch {
                alert("Failed to create folder");
            }
        }
    };

    const handleRenameFolder = async (id: string, currentName: string) => {
        setRenamingFolderId(id);
        setRenamingFolderName(currentName);
    };

    const handleConfirmRename = async () => {
        if (!renamingFolderId) return;

        if (renamingFolderName.length < 3) {
            alert("Folder name must be at least 3 characters");
            return;
        }

        const res = await fetch(`${API_BASE}/folders/${renamingFolderId}`, {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: renamingFolderName }),
            credentials: "include"
        });

        if (res.ok) {
            setRenamingFolderId(null);
            setRenamingFolderName("");
            await fetchFolders();
        } else {
            try {
                const error = await res.json();
                alert(error.message || "Failed to rename folder");
            } catch {
                alert("Failed to rename folder");
            }
        }
    };

    const handleDeleteFolder = async (id: string) => {
        if (!confirm("Delete folder? Files inside will be moved out, not deleted.")) return;
        await fetch(`${API_BASE}/folders/${id}`, { method: "DELETE", credentials: "include" });
        setFolders(folders.filter(f => f.id !== id));
        if (currentFolder?.id === id) setCurrentFolder(null);
    };

    const handleUpload = async () => {
        const input = document.createElement("input");
        input.type = "file";
        input.onchange = async () => {
            if (!input.files?.length) return;
            const formData = new FormData();
            formData.append("file", input.files[0]);

            const res = await fetch(`${API_BASE}/files`, { method: "POST", body: formData, credentials: "include" });
            if (res.ok) {
                setFilePage(1);
                await fetchFiles();
            } else {
                alert("Upload failed");
            }
        };
        input.click();
    };

    const handleLogout = () => {
        document.cookie.split(";").forEach((c) => {
            document.cookie = c
                .replace(/^ +/, "")
                .replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
        });
        router.push("/");
    };

    const toggleFileSelect = (id: string) => {
        setSelectedFiles(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

    const handleMoveToFolder = async (folderId: string) => {
        if (selectedFiles.size === 0) return;

        const res = await fetch(`${API_BASE}/folders/${folderId}/add`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ files: Array.from(selectedFiles) }),
            credentials: "include"
        });

        if (res.ok) {
            setSelectedFiles(new Set());
            setShowMoveModal(false);
            await fetchFiles();
            await fetchFolders();
        } else {
            try {
                const error = await res.json();
                alert(error.message || "Failed to move files");
            } catch {
                alert("Failed to move files");
            }
        }
    };

    const handleRemoveFromFolder = async (fileId: string) => {
        if (!currentFolder) return;

        const res = await fetch(`${API_BASE}/folders/${currentFolder.id}/remove`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ files: [fileId] }),
            credentials: "include"
        });

        if (res.ok) {
            await fetchFolders();
        }
    };

    const getImageUrl = (file: FileItem) => {
        if (file.accessibleURL) {
            return `${API_BASE}/proxy/image?url=${encodeURIComponent(file.accessibleURL)}`;
        }
        const fullUrl = `${cdnPrefix}/${file.cdn_file_name}`;
        return `${API_BASE}/proxy/image?url=${encodeURIComponent(fullUrl)}`;
    };

    const isImage = (mimeType: string) => mimeType?.startsWith("image/");

    const SortButton = ({ column, label }: { column: SortColumn; label: string }) => (
        <button
            onClick={() => handleSort(column)}
            className={`px-3 py-1.5 rounded-lg text-sm transition-colors ${sortColumn === column
                ? "bg-[#6c5ce7] text-white"
                : "bg-white/5 text-zinc-400 hover:bg-white/10"
                }`}
        >
            {label}
            {sortColumn === column && (
                <span className="ml-1">{sortDirection === "desc" ? "↓" : "↑"}</span>
            )}
        </button>
    );

    if (loading) return <div className="min-h-screen flex items-center justify-center text-[#6c5ce7]">Loading...</div>;
    if (!user) return null;

    return (
        <div className="flex h-screen p-4 gap-4 overflow-hidden">
            <aside className="w-64 glass-panel rounded-2xl flex flex-col p-6">
                <div className="mb-8"><h2 className="text-2xl font-bold tracking-wide">NestRip</h2></div>
                <nav className="flex-1">
                    <ul className="space-y-2">
                        {["overview", "files", "folders"].map(tab => (
                            <li key={tab}
                                onClick={() => { setActiveTab(tab); setCurrentFolder(null); setSelectedFiles(new Set()); }}
                                className={`p-3 rounded-xl cursor-pointer capitalize transition-colors ${activeTab === tab ? "bg-[#6c5ce7] text-white" : "hover:bg-white/5 text-zinc-400"}`}
                            >
                                {tab}
                            </li>
                        ))}
                    </ul>
                </nav>
                <div className="pt-4 border-t border-white/10">
                    <div className="mb-4">
                        <p className="text-sm font-semibold">{user.name}</p>
                        <p className="text-xs text-zinc-500 truncate">{user.email}</p>
                    </div>
                    <button onClick={handleLogout} className="w-full bg-white/5 hover:bg-red-500/20 text-zinc-400 hover:text-red-400 transition-colors py-2 rounded-lg text-sm">
                        Logout
                    </button>
                </div>
            </aside>

            <main className="flex-1 glass-panel rounded-2xl p-8 overflow-y-auto">
                {activeTab === "overview" && (
                    <div className="animate-fade-in">
                        <h1 className="text-3xl font-bold mb-6">Welcome back, {user.name}</h1>
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div className="bg-white/5 p-6 rounded-2xl border border-white/10">
                                <h3 className="text-zinc-400 text-sm mb-2">Total Uploads</h3>
                                <p className="text-4xl font-bold">{stats?.uploads ?? 0}</p>
                            </div>
                            <div className="bg-white/5 p-6 rounded-2xl border border-white/10">
                                <h3 className="text-zinc-400 text-sm mb-2">Storage Used</h3>
                                <p className="text-4xl font-bold">{formatBytes(stats?.storageUsed ?? 0)}</p>
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === "files" && !currentFolder && (
                    <div className="animate-fade-in">
                        <div className="flex justify-between items-center mb-4">
                            <div>
                                <h1 className="text-2xl font-bold">My Files</h1>
                                <p className="text-zinc-500 text-sm">{totalFiles} files total</p>
                            </div>
                            <div className="flex gap-2">
                                {selectedFiles.size > 0 && (
                                    <button onClick={() => setShowMoveModal(true)} className="bg-[#00cec9] hover:bg-[#00b5ad] text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">
                                        Move {selectedFiles.size} to Folder
                                    </button>
                                )}
                                <button onClick={handleUpload} className="bg-[#6c5ce7] hover:bg-[#a29bfe] text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">
                                    Upload File
                                </button>
                            </div>
                        </div>

                        <div className="flex gap-2 mb-4 flex-wrap">
                            <span className="text-zinc-500 text-sm py-1.5">Sort by:</span>
                            <SortButton column="created_at" label="Date" />
                            <SortButton column="views" label="Views" />
                            <SortButton column="size" label="Size" />
                            <SortButton column="filename" label="Name" />
                        </div>

                        {folders.length > 0 && (
                            <div className="mb-6">
                                <h3 className="text-sm text-zinc-500 mb-2">Quick Access Folders</h3>
                                <div className="flex gap-2 flex-wrap">
                                    {folders.slice(0, 5).map(folder => (
                                        <button
                                            key={folder.id}
                                            onClick={() => setCurrentFolder(folder)}
                                            className="flex items-center gap-2 px-3 py-2 bg-white/5 hover:bg-white/10 rounded-lg text-sm transition-colors"
                                        >
                                            <svg className="w-4 h-4 text-[#6c5ce7]" fill="currentColor" viewBox="0 0 20 20">
                                                <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
                                            </svg>
                                            {folder.name}
                                            <span className="text-zinc-500">({folder.file_count})</span>
                                        </button>
                                    ))}
                                </div>
                            </div>
                        )}

                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 mb-6">
                            {files.map(file => (
                                <div key={file.id} className={`bg-white/5 rounded-xl border overflow-hidden group relative ${selectedFiles.has(file.id) ? 'border-[#6c5ce7] ring-2 ring-[#6c5ce7]/50' : 'border-white/10'}`}>
                                    <div
                                        className="absolute top-2 left-2 z-10"
                                        onClick={(e) => { e.preventDefault(); toggleFileSelect(file.id); }}
                                    >
                                        <div className={`w-5 h-5 rounded border-2 cursor-pointer flex items-center justify-center ${selectedFiles.has(file.id) ? 'bg-[#6c5ce7] border-[#6c5ce7]' : 'border-white/30 hover:border-white/50'}`}>
                                            {selectedFiles.has(file.id) && (
                                                <svg className="w-3 h-3 text-white" fill="currentColor" viewBox="0 0 20 20">
                                                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                                                </svg>
                                            )}
                                        </div>
                                    </div>
                                    <div className="aspect-square bg-black/20 flex items-center justify-center overflow-hidden">
                                        {isImage(file.mime_type) ? (
                                            <img
                                                src={getImageUrl(file)}
                                                alt={file.filename}
                                                className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                                                loading="lazy"
                                                referrerPolicy="no-referrer"
                                                onError={(e) => {
                                                    console.error("Failed to load image");
                                                    const parent = e.currentTarget.parentElement;
                                                    if (parent) {
                                                        e.currentTarget.style.display = 'none';
                                                        const fallback = parent.querySelector('[data-image-fallback]');
                                                        if (fallback) fallback.classList.remove('hidden');
                                                    }
                                                }}
                                            />
                                        ) : null}
                                        {isImage(file.mime_type) && (
                                            <div className="text-zinc-500 text-xs text-center p-2 hidden" data-image-fallback>
                                                {file.mime_type?.split('/')[1]?.toUpperCase() || 'IMAGE'}
                                            </div>
                                        )}
                                        {!isImage(file.mime_type) && (
                                            <div className="text-zinc-500 text-xs text-center p-2">
                                                {file.mime_type?.split('/')[1]?.toUpperCase() || 'FILE'}
                                            </div>
                                        )}
                                    </div>
                                    <div className="p-3">
                                        <p className="text-sm truncate text-zinc-300" title={file.original_filename || file.filename}>
                                            {file.filename}
                                        </p>
                                        <div className="flex justify-between items-center mt-2 text-xs text-zinc-500">
                                            <span>{formatBytes(file.size)}</span>
                                            <span>{file.views} views</span>
                                        </div>
                                        <button
                                            onClick={() => handleDeleteFile(file.id)}
                                            className="w-full mt-2 text-red-400 hover:text-red-300 hover:bg-red-500/10 py-1 rounded text-xs transition-colors"
                                        >
                                            Delete
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="flex justify-center items-center gap-4">
                            <button
                                onClick={() => setFilePage(p => Math.max(1, p - 1))}
                                disabled={filePage === 1}
                                className="px-4 py-2 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                            >
                                Previous
                            </button>
                            <span className="text-zinc-400">Page {filePage} of {totalPages}</span>
                            <button
                                onClick={() => setFilePage(p => Math.min(totalPages, p + 1))}
                                disabled={filePage === totalPages}
                                className="px-4 py-2 rounded-lg bg-white/5 hover:bg-white/10 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                            >
                                Next
                            </button>
                        </div>
                    </div>
                )}

                {activeTab === "files" && currentFolder && (
                    <div className="animate-fade-in">
                        <div className="flex justify-between items-center mb-6">
                            <div>
                                <button onClick={() => setCurrentFolder(null)} className="text-zinc-500 hover:text-zinc-300 text-sm mb-2 flex items-center gap-1">
                                    ← Back to All Files
                                </button>
                                <h1 className="text-2xl font-bold flex items-center gap-2">
                                    <svg className="w-6 h-6 text-[#6c5ce7]" fill="currentColor" viewBox="0 0 20 20">
                                        <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
                                    </svg>
                                    {currentFolder.name}
                                </h1>
                                <p className="text-zinc-500 text-sm">{currentFolder.file_count} files in folder</p>
                            </div>
                        </div>

                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4 mb-6">
                            {currentFolder.files?.map(file => (
                                <div key={file.id} className="bg-white/5 rounded-xl border border-white/10 overflow-hidden group">
                                    <div className="aspect-square bg-black/20 flex items-center justify-center overflow-hidden">
                                        {isImage(file.mime_type) ? (
                                            <img
                                                src={`${API_BASE}/proxy/image?url=${encodeURIComponent(`${cdnPrefix}/${file.cdn_file_name}`)}`}
                                                alt={file.original_filename}
                                                className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                                                loading="lazy"
                                                referrerPolicy="no-referrer"
                                                onError={(e) => {
                                                    console.error("Failed to load image");
                                                    const parent = e.currentTarget.parentElement;
                                                    if (parent) {
                                                        e.currentTarget.style.display = 'none';
                                                        const fallback = parent.querySelector('[data-image-fallback]');
                                                        if (fallback) fallback.classList.remove('hidden');
                                                    }
                                                }}
                                            />
                                        ) : null}
                                        {isImage(file.mime_type) && (
                                            <div className="text-zinc-500 text-xs text-center p-2 hidden" data-image-fallback>
                                                {file.mime_type?.split('/')[1]?.toUpperCase() || 'IMAGE'}
                                            </div>
                                        )}
                                        {!isImage(file.mime_type) && (
                                            <div className="text-zinc-500 text-xs text-center p-2">
                                                {file.mime_type?.split('/')[1]?.toUpperCase() || 'FILE'}
                                            </div>
                                        )}
                                    </div>
                                    <div className="p-3">
                                        <p className="text-sm truncate text-zinc-300" title={file.original_filename}>
                                            {file.original_filename}
                                        </p>
                                        <div className="flex justify-between items-center mt-2 text-xs text-zinc-500">
                                            <span>{formatBytes(file.size)}</span>
                                        </div>
                                        <button
                                            onClick={() => handleRemoveFromFolder(file.id)}
                                            className="w-full mt-2 text-orange-400 hover:text-orange-300 hover:bg-orange-500/10 py-1 rounded text-xs transition-colors"
                                        >
                                            Remove from Folder
                                        </button>
                                    </div>
                                </div>
                            ))}
                            {(!currentFolder.files || currentFolder.files.length === 0) && (
                                <div className="col-span-full text-center py-12 text-zinc-500">
                                    No files in this folder yet. Select files and move them here!
                                </div>
                            )}
                        </div>
                    </div>
                )}

                {activeTab === "folders" && (
                    <div className="animate-fade-in">
                        <div className="flex justify-between items-center mb-6">
                            <div>
                                <h1 className="text-2xl font-bold">My Folders</h1>
                                <p className="text-zinc-500 text-sm">{folders.length} folders</p>
                            </div>
                            <button onClick={handleCreateFolder} className="bg-[#6c5ce7] hover:bg-[#a29bfe] text-white px-4 py-2 rounded-lg text-sm font-medium transition-colors">
                                Create Folder
                            </button>
                        </div>

                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                            {folders.map(folder => (
                                <div key={folder.id} className="bg-white/5 rounded-xl border border-white/10 p-4 hover:bg-white/10 transition-colors">
                                    {renamingFolderId === folder.id ? (
                                        <div className="mb-3 flex gap-2">
                                            <input
                                                type="text"
                                                value={renamingFolderName}
                                                onChange={(e) => setRenamingFolderName(e.target.value)}
                                                className="flex-1 px-2 py-1 bg-white/10 border border-white/20 rounded text-sm text-white"
                                                autoFocus
                                            />
                                            <button
                                                onClick={handleConfirmRename}
                                                className="px-2 py-1 bg-[#6c5ce7] hover:bg-[#a29bfe] text-white rounded text-xs"
                                            >
                                                Save
                                            </button>
                                        </div>
                                    ) : (
                                        <div
                                            className="flex items-center gap-3 mb-3 cursor-pointer"
                                            onClick={() => { setActiveTab("files"); setCurrentFolder(folder); }}
                                        >
                                            <div className="w-10 h-10 bg-[#6c5ce7]/20 rounded-lg flex items-center justify-center">
                                                <svg className="w-5 h-5 text-[#6c5ce7]" fill="currentColor" viewBox="0 0 20 20">
                                                    <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
                                                </svg>
                                            </div>
                                            <div className="flex-1 min-w-0">
                                                <p className="text-sm font-medium truncate">{folder.name}</p>
                                                <p className="text-xs text-zinc-500">{folder.file_count} files</p>
                                            </div>
                                        </div>
                                    )}
                                    <div className="flex justify-between items-center text-xs text-zinc-500">
                                        <span>{folder.public ? "Public" : "Private"}</span>
                                        <div className="flex gap-2">
                                            <button
                                                onClick={() => handleRenameFolder(folder.id, folder.name)}
                                                className="text-blue-400 hover:text-blue-300"
                                            >
                                                Rename
                                            </button>
                                            <button
                                                onClick={() => handleDeleteFolder(folder.id)}
                                                className="text-red-400 hover:text-red-300"
                                            >
                                                Delete
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))}
                            {folders.length === 0 && (
                                <div className="col-span-full text-center py-12 text-zinc-500">
                                    No folders yet. Create one to organize your files!
                                </div>
                            )}
                        </div>
                    </div>
                )}
            </main>

            {showMoveModal && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50" onClick={() => setShowMoveModal(false)}>
                    <div className="bg-[#1a1a2e] rounded-2xl p-6 w-full max-w-md border border-white/10" onClick={e => e.stopPropagation()}>
                        <h2 className="text-xl font-bold mb-4">Move to Folder</h2>
                        <p className="text-zinc-400 text-sm mb-4">Select a folder to move {selectedFiles.size} file(s) to:</p>
                        <div className="space-y-2 max-h-64 overflow-y-auto">
                            {folders.map(folder => (
                                <button
                                    key={folder.id}
                                    onClick={() => handleMoveToFolder(folder.id)}
                                    className="w-full flex items-center gap-3 p-3 bg-white/5 hover:bg-white/10 rounded-lg transition-colors text-left"
                                >
                                    <svg className="w-5 h-5 text-[#6c5ce7]" fill="currentColor" viewBox="0 0 20 20">
                                        <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
                                    </svg>
                                    <span>{folder.name}</span>
                                    <span className="text-zinc-500 text-sm ml-auto">{folder.file_count} files</span>
                                </button>
                            ))}
                            {folders.length === 0 && (
                                <p className="text-zinc-500 text-center py-4">No folders. Create one first!</p>
                            )}
                        </div>
                        <button
                            onClick={() => setShowMoveModal(false)}
                            className="w-full mt-4 py-2 bg-white/5 hover:bg-white/10 rounded-lg text-sm transition-colors"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

function formatBytes(bytes: number, decimals = 2) {
    if (!+bytes) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
}