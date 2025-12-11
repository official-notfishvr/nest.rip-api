
import { type NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
    const searchParams = request.nextUrl.searchParams;
    const code = searchParams.get("code");

    if (!code) {
        return NextResponse.json({ error: "No code provided" }, { status: 400 });
    }

    // Redirect to Backend to process the code
    // The backend will set the cookie and redirect back to /dashboard
    return NextResponse.redirect(`https://skid-api.gtag-api.win/oauth/callback?code=${code}`);
}
