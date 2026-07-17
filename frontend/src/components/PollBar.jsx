import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Cell, LabelList, ResponsiveContainer } from 'recharts'

const BAR_COLORS = ['#f2b84b', '#4fd1c5', '#ef6a5a', '#8a7ff0', '#f2b84b', '#4fd1c5']

/**
 * Recharts bar chart for the live results page. Re-renders whenever `options`
 * changes, which happens every time PollHub pushes a "resultsUpdated" event -
 * Recharts animates the bar height/width delta on its own.
 */
export default function PollBars({ options }) {
  if (options.length === 0) {
    return <p className="helper">No votes yet — be the first to share this poll.</p>
  }

  const data = options.map((o) => ({
    name: o.text.length > 18 ? `${o.text.slice(0, 17)}…` : o.text,
    fullName: o.text,
    votes: o.count,
    percentage: o.percentage,
  }))

  return (
    <ResponsiveContainer width="100%" height={Math.max(180, data.length * 56)}>
      <BarChart
        data={data}
        layout="vertical"
        margin={{ top: 4, right: 48, left: 8, bottom: 4 }}
      >
        <CartesianGrid strokeDasharray="3 3" stroke="#34324a" horizontal={false} />
        <XAxis type="number" stroke="#9b98b3" tick={{ fontSize: 12, fill: '#9b98b3' }} allowDecimals={false} />
        <YAxis
          type="category"
          dataKey="name"
          width={140}
          stroke="#9b98b3"
          tick={{ fontSize: 13, fill: '#ece9f7' }}
        />
        <Tooltip
          cursor={{ fill: 'rgba(242,184,75,0.08)' }}
          contentStyle={{ background: '#262538', border: '1px solid #34324a', borderRadius: 8 }}
          labelStyle={{ color: '#ece9f7' }}
          formatter={(value, _name, props) => [`${value} vote(s) · ${props.payload.percentage}%`, props.payload.fullName]}
        />
        <Bar dataKey="votes" radius={[0, 6, 6, 0]} isAnimationActive animationDuration={450}>
          {data.map((_, index) => (
            <Cell key={index} fill={BAR_COLORS[index % BAR_COLORS.length]} />
          ))}
          <LabelList dataKey="votes" position="right" fill="#ece9f7" fontSize={12} />
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  )
}
