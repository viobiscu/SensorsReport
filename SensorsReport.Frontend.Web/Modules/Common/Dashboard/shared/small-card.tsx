import { faIcon, resolveUrl } from "@serenity-is/corelib";

export const SmallCard = ({ icon, caption, url, value, colClass, colorClass }: { icon: string, caption: string, url?: string, value: any, colClass?: string, colorClass?: string }) => (
    <div class={colClass ?? "col-lg-3 col-sm-6"}>
        <div class={["card s-dashboard-card-sm", colorClass]}>
            <div class="card-body"><h3>{value}</h3><p>{caption}</p></div>
            <div class="icon"><i class={icon}></i></div>
            {url ? <a href={resolveUrl(url)} class="card-footer">More info <i class={faIcon("arrow-circle-right")}></i></a> : null }
        </div>
    </div>
);
